// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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
            string connStr
        )
        {
            services.AddDbContext<ApplicationDbContext, ApplicationDbContextPostgreSQL>(
                builder => builder.UseNpgsql(connStr,
                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
            );

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