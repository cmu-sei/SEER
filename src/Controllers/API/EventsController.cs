// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;

namespace Seer.Controllers.API
{
    [Route("/api/[controller]/")]
    public class EventsController : BaseController
    {
        public EventsController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<Event>> Index(CancellationToken ct)
        {
            return await _db.Events.Include(o => o.Details).Include(o => o.AssignedTo)
                .OrderBy(x => x.AssessmentId).ThenBy(o => o.DisplayOrder)
                .ToListAsync(ct);
        }

    }
}