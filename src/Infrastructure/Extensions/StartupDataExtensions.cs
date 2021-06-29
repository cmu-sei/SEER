/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seer.Infrastructure.Data;

namespace Seer.Infrastructure.Extensions
{
    public static class StartupDataExtensions
    {
        public static IServiceCollection AddApplicationData(
            this IServiceCollection services,
            string provider,
            string connStr
        )
        {
            switch (provider)
            {

                case "SqlServer":
                    services.AddDbContext<ApplicationDbContext, ApplicationDbContextSqlServer>(
                        builder => builder.UseSqlServer(connStr,
                            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
                    );
                    break;
                case "SqlLite":
                    services.AddDbContext<ApplicationDbContext, ApplicationDbContextSqlServer>(
                        builder => builder.UseSqlite(connStr,
                            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
                    );
                    break;
                case "PostgreSQL":
                    services.AddDbContext<ApplicationDbContext, ApplicationDbContextPostgreSQL>(
                        builder => builder.UseNpgsql(connStr,
                            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
                    );
                    break;

                default:
                    services.AddDbContext<ApplicationDbContext, ApplicationDbContextInMemory>(
                        builder => builder.UseInMemoryDatabase("SEER_DB")
                    );
                    break;
            }

            // Auto-discover from EntityStore and IEntityStore pattern
            foreach (var type in
                Assembly.GetExecutingAssembly().ExportedTypes
                    .Where(t => t.Namespace == "Application.Data" && t.Name.EndsWith("Store") && t.IsClass))
            {
                var ti = type.GetInterfaces().FirstOrDefault(i => i.Name == $"I{type.Name}");

                if (ti != null)
                {
                    services.AddScoped(ti, type);
                }
            }

            return services;
        }
    }
}
