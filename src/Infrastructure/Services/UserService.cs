// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLog;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public UserService(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            this._db = dbContext;
            this._userManager = userManager;
        }
        
        public async Task CreateAccountAsync(IdentityUser user)
        {
            _log.Debug($"Creating new user: {user.Email}");
            
            var dbUser = await this._userManager.FindByEmailAsync(user.Email);
            if (dbUser == null)
            {
                _log.Debug($"User {user.Email} does not exist, creating...");
            
                dbUser = new User { Email = user.Email, UserName = user.Email, Created = DateTime.UtcNow };
                var id = await this._userManager.CreateAsync(dbUser, "Scotty@@1!");
                if (!id.Succeeded)
                {
                    _log.Debug(string.Join("\n", id.Errors));
                    throw new Exceptions.UserCreationException($"User {user.Email} could not be created. Errors: {id.Errors}");
                }
            
                if (Program.Configuration.DefaultAdminAccounts.Any(user.Email.Split('@')[0].Contains))
                {
                    _log.Debug($"User {dbUser.Email} is admin...");
                    if (!this._userManager.IsInRoleAsync(dbUser, User.Role.Admin.ToString()).Result)
                    {
                        _log.Debug($"Adding roles...");
                        await this._userManager.AddToRoleAsync(dbUser, User.Role.Admin.ToString());
                        var groups = _db.Groups.Where(o => o.Status == ActiveStatus.Active).ToArray();
                        foreach (var group in groups)
                        {
                            _db.GroupUsers.Add(new GroupUser { GroupId = group.Id, UserId = user.Id });
                            _log.Trace($"adding user {dbUser.Email} to group: {group.Name}");
            
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            
            if (user == null)
            {
                throw new Exceptions.UserCreationException("User could not be created");
            }
        }

        public async Task<User> GetUserAsync(string userName)
        {
            var user = await this._db.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user != null) return user; //user exists

            userName = userName.MakeEmailAddress();
            user = new User { UserName = userName, Email = userName };
            this._db.Users.Add(user);
            await this._db.SaveChangesAsync();

            return user;
        }
    }
}