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

                    await context.Campaigns.AddAsync(campaign);
                    await context.SaveChangesAsync();

                    foreach (var g in seedValue.Groups)
                    {
                        var group = new Group { Name = g.Group, Status = ActiveStatus.Active };
                        await context.Groups.AddAsync(group);
                        await context.SaveChangesAsync();
                        foreach (var assessment in g.Assessments)
                        {
                            var o = await context.Operations.FirstOrDefaultAsync(x => x.Name == assessment.Name);
                            await context.Assessments.AddAsync(
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