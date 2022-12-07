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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services
{
    public static class UserService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static async Task CreateAccountAsync(TokenResponseReceivedContext ctx, Configuration _conf)
        {
            var token = ctx.TokenEndpointResponse.AccessToken;

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
            var name = jwtSecurityToken?.Claims.First(claim => claim.Type == "name").Value;
            var ssoId = jwtSecurityToken?.Claims.First(claim => claim.Type == "sub").Value;
            var email = name.MakeEmailAddress();

            if (ssoId == null)
            {
                throw new Exception("Identity ssoId not found");
            }

            _log.Debug($"{ssoId}: Processing SSO");

            var db = ctx.HttpContext.RequestServices.GetService<ApplicationDbContext>();
            if (db == null) throw new Exception("Cannot get handle to database service");
            var userManager = ctx.HttpContext.RequestServices.GetService<UserManager<User>>();
            if (userManager == null) throw new Exceptions.UserCreationException("Cannot get handle to UserManager service");

            var user = await userManager.FindByIdAsync(ssoId);
            if (user == null)
            {
                _log.Debug($"{ssoId}: User does not exist attempting to find by email");
                user = await userManager.FindByEmailAsync(email);
            }

            if (user == null)
            {
                _log.Debug($"{ssoId}: User does not exist, creating... - {name} {email}");
                //user does not exist, create user

                user = new User { Email = email, UserName = email, Created = DateTime.UtcNow, Id = ssoId, OAuthId = ssoId, FirstName = name};
                var id = await userManager.CreateAsync(user, "Scotty@@1!");
                if (!id.Succeeded)
                {
                    _log.Debug(string.Join("\n", id.Errors));
                    throw new Exceptions.UserCreationException($"User {user.Email} could not be created");
                }

                if (_conf.DefaultAdminAccounts.Any(s => s.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.Debug($"{ssoId}: User is admin...");
                    if (!userManager.IsInRoleAsync(user, User.Role.Admin.ToString()).Result)
                    {
                        _log.Debug($"{ssoId}: User is admin, adding roles...");
                        await userManager.AddToRoleAsync(user, User.Role.Admin.ToString());
                        var groups = db.Groups.Where(o => o.Status == ActiveStatus.Active);
                        foreach (var group in groups)
                        {
                            if (!db.GroupUsers.Any(o => o.GroupId == group.Id && o.UserId == ssoId))
                            {
                                db.GroupUsers.Add(new GroupUser { GroupId = group.Id, UserId = user.Id });
                                _log.Trace($"adding user {email} to group: {group.Name}");
                            }

                            await db.SaveChangesAsync();
                        }
                    }
                }
            }

            if (user == null)
            {
                throw new Exceptions.UserCreationException("User could not be created");
            }

            //user exists, login
            db.UserJwts.Add(new UserJwt { SsoId = ssoId, Jwt = token, Name = name, Email = email });
            await db.SaveChangesAsync();

            _log.Debug($"{ssoId}: Checking roles...");

            //add groups to local auth cookie
            foreach (var role in await userManager.GetRolesAsync(user))
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Role, role)
                };
                var appIdentity = new ClaimsIdentity(claims);

                ctx.Principal?.AddIdentity(appIdentity);
            }

            if (_conf.Squire.Enabled)
            {
                await new SquireService(db).Run(user);
            }
        }

        public static async Task<User> GetUserAsync(ApplicationDbContext dbContext, string userName)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user != null) return user; //user exists

            userName = userName.MakeEmailAddress();
            user = new User { UserName = userName, Email = userName };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }
    }
}