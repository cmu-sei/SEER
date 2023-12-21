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

namespace Seer.Controllers.API
{
    [Route("api/assessments/")]
    public class AssessmentsController : BaseController
    {
        public AssessmentsController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Assessment>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _db.Assessments.OrderByDescending(o => o.Created).ToListAsync(ct);
            return Ok(list);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Assessment), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create(Assessment model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("ModelState error");
            }

            try
            {
                _db.Assessments.Add(model);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok(model);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Assessment), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update(int id, Assessment model, CancellationToken ct)
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