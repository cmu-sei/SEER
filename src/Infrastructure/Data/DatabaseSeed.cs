// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Seer.Infrastructure.Enums;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Data
{
    public class DatabaseSeed
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager, ILogger<DatabaseSeed> logger)
        {
            await context.Database.EnsureCreatedAsync();
            //context.Database.Migrate();

            // Look for any users.
            if (context.Assessments.Any())
                return; // DB has been seeded

            SeedRoles(roleManager);

            var seedFile = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "config", Program.Configuration.Database.SeedFile);
            if (File.Exists(seedFile))
            {
                var seedValues = JsonConvert.DeserializeObject<IList<DefaultSeedValue>>(await File.ReadAllTextAsync(seedFile));
                foreach (var seedValue in seedValues)
                {
                    // Campaign / operation / assessment 
                    var operations = seedValue.Operations.Select(op => new Operation { Name = op }).ToList();
                    var campaign = new Campaign { Name = seedValue.Campaign, Operations = operations };

                    context.Campaigns.Add(campaign);
                    await context.SaveChangesAsync();

                    foreach (var g in seedValue.Groups)
                    {
                        var group = new Group { Name = g.Group, Status = ActiveStatus.Active };
                        context.Groups.Add(group);
                        await context.SaveChangesAsync();
                        foreach (var assessment in g.Assessments)
                        {
                            var o = await context.Operations.FirstOrDefaultAsync(x => x.Name == assessment.Name);
                            context.Assessments.Add(
                                new Assessment
                                {
                                    Name = assessment.Name,
                                    Status = ActiveStatus.Inactive,
                                    GroupId = group.Id,
                                    HasOrders = false,
                                    OperationId = o.Id
                                });
                        }
                    }

                    await context.SaveChangesAsync();
                }
            }
        }

        private static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Enum.GetValues(typeof(User.Role)))
            {
                object x;
                if (!roleManager.RoleExistsAsync(role.ToString()).Result)
                    x = roleManager.CreateAsync(new IdentityRole(role.ToString())).Result;
            }
        }
    }
}