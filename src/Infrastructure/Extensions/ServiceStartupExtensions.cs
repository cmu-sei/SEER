// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Seer.Infrastructure.Extensions
{
    public static class ServiceStartupExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services
        )
        {
            // Auto-discover from EntityService pattern
            foreach (var t in
                Assembly.GetExecutingAssembly().ExportedTypes
                    .Where(t => t.Namespace == "Seer.Services"))
            {
                services.AddScoped(t);
            }

            return services;
        }

        public static IMapperConfigurationExpression AddApplicationMaps(
            this IMapperConfigurationExpression cfg
        )
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
            return cfg;
        }
    }
}