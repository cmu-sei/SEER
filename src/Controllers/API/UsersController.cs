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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seer.Infrastructure.Data;

namespace Seer.Controllers.API
{
    [Route("api/users/")]
    public class UsersController : BaseController
    {
        private readonly IServiceProvider _serviceProvider;

        public UsersController(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            this._db = dbContext;
            this._serviceProvider = serviceProvider;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _db.Users.OrderByDescending(o => o.UserName).ToListAsync(ct);
            var userManager = this._serviceProvider.GetRequiredService<UserManager<User>>();
            foreach (var item in list)
            {
                var existingUser = await userManager.FindByIdAsync(item.Id);
                if (existingUser == null)
                {
                    continue;
                }

                item.Roles = await userManager.GetRolesAsync(existingUser);
                item.PasswordHash = "";
                item.SecurityStamp = "";
                item.ConcurrencyStamp = "";
            }

            list = list.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();

            return Ok(list);
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPost]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(User user, CancellationToken ct)
        {
            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);
            return Ok(user);
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(string id, User user, CancellationToken ct)
        {
            var existingUser = await _db.Users.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (existingUser == null)
            {
                return NotFound();
            }

            // only certain values can be changed
            existingUser.LastName = user.LastName;
            existingUser.FirstName = user.FirstName;
            existingUser.Rank = user.Rank;
            existingUser.DutyPosition = user.DutyPosition;
            existingUser.OAuthId = user.OAuthId;
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;

            _db.Users.Update(existingUser);
            await _db.SaveChangesAsync(ct);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (user == null)
            {
                return NotFound();
            }

            _db.Users.Remove(user);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpGet("{id}/roles")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RolesGet(string id)
        {
            var userManager = this._serviceProvider.GetRequiredService<UserManager<User>>();

            var existingUser = await userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            var roles = await userManager.GetRolesAsync(existingUser);

            return Ok(roles);
        }

        [HttpPost("{id}/roles/{role}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RolesAdd(string id, User.Role role)
        {
            var userManager = this._serviceProvider.GetRequiredService<UserManager<User>>();

            var existingUser = await userManager.FindByIdAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            if (!await userManager.IsInRoleAsync(existingUser, role.ToString()))
            {
                await userManager.AddToRoleAsync(existingUser, role.ToString());
            }

            return NoContent();
        }

        [HttpPost("{id}/roles/{role}/delete")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RolesRemove(string id, User.Role role)
        {
            var userManager = this._serviceProvider.GetRequiredService<UserManager<User>>();

            var existingUser = await userManager.FindByIdAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            if (await userManager.IsInRoleAsync(existingUser, role.ToString()))
            {
                await userManager.RemoveFromRoleAsync(existingUser, role.ToString());
            }

            return NoContent();
        }

        [HttpPut("{id}/roles")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RolesUpdate(string id, IEnumerable<User.Role> roles)
        {
            var userManager = this._serviceProvider.GetRequiredService<UserManager<User>>();

            var existingUser = await userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            //remove all roles
            foreach (var role in await userManager.GetRolesAsync(existingUser))
            {
                await userManager.RemoveFromRoleAsync(existingUser, role);
            }

            //now add them back
            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(existingUser, role.ToString()))
                {
                    await userManager.AddToRoleAsync(existingUser, role.ToString());
                }
            }
            return Ok(existingUser);
        }
    }
}