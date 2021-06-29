/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Seer.Infrastructure.Extensions
{
    public static class DbSetExtension
    {
        public static void AddOrUpdate<T>(this DbSet<T> dbSet, T data) where T : class
        {
            var context = dbSet.GetContext();
            var ids = context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(x => x.Name).ToList();

            var t = typeof(T);
            var keyFields = new List<PropertyInfo>();

            foreach (var propertyInfo in t.GetProperties())
            {
                var keyAttr = ids.Contains(propertyInfo.Name);
                if (keyAttr)
                {
                    keyFields.Add(propertyInfo);
                }
            }
            if (keyFields.Count <= 0)
            {
                throw new Exception($"{t.FullName} does not have a KeyAttribute field. Unable to exec AddOrUpdate call.");
            }
            var entities = dbSet.AsNoTracking().ToList();
            foreach (var keyField in keyFields)
            {
                var keyVal = keyField.GetValue(data);
                entities = entities.Where(p => p != null && p.GetType().GetProperty(keyField.Name).GetValue(p).Equals(keyVal)).ToList();
            }
            var dbVal = entities.FirstOrDefault();
            if (dbVal != null)
            {
                context.Entry(dbVal).CurrentValues.SetValues(data);
                context.Entry(dbVal).State = EntityState.Modified;
                return;
            }
            dbSet.Add(data);
        }
    }

    public static class GetContextsExtension
    {
        public static DbContext GetContext<TEntity>(this DbSet<TEntity> dbSet)
            where TEntity : class
        {
            return (DbContext)dbSet
                .GetType().GetTypeInfo()
                .GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(dbSet);
        }
    }
}
