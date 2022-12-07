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