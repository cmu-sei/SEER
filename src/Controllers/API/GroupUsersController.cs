// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;

namespace Seer.Controllers.API
{
    [Route("api/groups")]
    public class GroupUsersController : BaseController
    {
        public GroupUsersController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpPost("{groupId}/users/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UserAdd(int groupId, string userId, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("ModelState error");
            }

            if (_db.GroupUsers.Any(x => x.UserId == userId && x.GroupId == groupId))
            {
                return NoContent();
            }

            try
            {
                _db.GroupUsers.Add(new GroupUser { UserId = userId, GroupId = groupId });
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return NoContent();
        }

        [HttpPost("{groupId}/users/{userId}/delete")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UserRemove(int groupId, string userId, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("ModelState error");
            }

            try
            {
                var item = _db.GroupUsers.FirstOrDefault(o => o.GroupId == groupId && o.UserId == userId);
                if (item != null)
                {
                    _db.GroupUsers.Remove(item);
                    await _db.SaveChangesAsync(ct);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return NoContent();
        }
    }
}