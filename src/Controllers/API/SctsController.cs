// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Controllers.API
{
    [Route("api/scts/")]
    public class SctsController : BaseController
    {
        public SctsController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpGet("{assessmentid}")]
        public async Task<IEnumerable<METItemSCT>> GetSCTs(int assessmentid, CancellationToken ct)
        {
            var list = new List<METItemSCT>();

            var mets = await this._db.METs
                .Include(x => x.METItems)
                .ThenInclude(x => x.METSCTs)
                .Where(x => x.AssessmentId == assessmentid)
                .ToListAsync(ct);

            foreach (var metItem in mets.SelectMany(met => met.METItems))
            {
                list.AddRange(metItem.METSCTs);
            }
            return list;
        }
    }
}