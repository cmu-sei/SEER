// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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
    [Route("api/execution/")]
    public class ExecutionController : BaseController
    {
        public ExecutionController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Group>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _db.CatalogEvents.Include(o => o.Details)
                .OrderByDescending(o => o.Name)
                .ToListAsync(ct);
            return Ok(list);
        }

        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<Group>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateBlank(TimelineItemRequest request, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == this.UserId, ct);

            var evt = new Event();

            if (request.CatalogId > 0)
            {
                var item = await _db.CatalogEvents.Include(o => o.Details)
                    .FirstOrDefaultAsync(x => x.Id == request.CatalogId, ct);
                evt = new Event(item);
            }

            evt.AssignedTo = user;
            evt.AssessmentId = request.AssessmentId;
            evt.DisplayOrder = request.DisplayOrder;

            this._db.Events.Add(evt);
            await this._db.SaveChangesAsync(ct);
            return Ok(evt);
        }

        public class TimelineItemRequest
        {
            public int AssessmentId { get; set; }
            public int CatalogId { get; set; }
            public int DisplayOrder { get; set; }
        }
    }
}