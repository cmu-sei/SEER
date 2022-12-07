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