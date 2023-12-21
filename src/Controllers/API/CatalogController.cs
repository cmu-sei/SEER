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
    [Route("api/catalog/")]
    public class CatalogController : BaseController
    {
        public CatalogController(ApplicationDbContext dbContext)
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
    }
}