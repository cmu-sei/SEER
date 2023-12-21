// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.Services;

namespace Seer
{
    public static class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static Configuration Configuration { get; private set; }
        public static void Main(string[] args)
        {
            _log.Warn("System coming online...");
            Configuration = ConfigurationService.Load();

            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var dbInitializerLogger = services.GetRequiredService<ILogger<DatabaseSeed>>();

                    DatabaseSeed.Initialize(context, userManager, roleManager, dbInitializerLogger).Wait();
                }
                catch (Exception ex)
                {
                    _log.Fatal(ex, $"An error occurred while seeding the database: {ex.Message}");
                }
            }

            host.Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
