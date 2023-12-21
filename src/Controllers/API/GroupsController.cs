// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Controllers.API
{
    [Route("api/groups/")]
    public class GroupsController : BaseController
    {
        public GroupsController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Group>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _db.Groups.Include(o => o.Users).Where(o => o.Status == ActiveStatus.Active).OrderBy(o => o.Name)
                .ToListAsync(ct);
            return Ok(list);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Group), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create(Group model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("ModelState error");
            }

            try
            {
                _db.Groups.Add(model);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(model);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Group), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(int id, Group model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("ModelState error");
            }

            try
            {
                _db.Update(model);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(model);
        }
    }
}