// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.IO;
using Microsoft.Extensions.Configuration;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services
{
    public class ConfigurationService
    {
        public static Configuration Load()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();
            var appConfig = new Configuration();
            config.GetSection("ApplicationConfiguration").Bind(appConfig);

            //TODO: HACK: 
            appConfig.Host = config.GetValue<string>("JwtIssuerOptions:Audience");

            return appConfig;
        }
    }
}