// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class ExecutionController : BaseController
    {
        public ExecutionController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet("summary")]
        public async Task<ActionResult> Summary()
        {
            var ct = new CancellationToken();
            var list = await _db.Events.Include(o => o.Details).Include(o => o.AssignedTo)
                .OrderBy(o => o.DisplayOrder).Where(x => x.AssessmentId == this.AssessmentId)
                .ToListAsync(ct);
            return View(list);
        }

        [HttpGet("deployment")]
        public async Task<ActionResult> Deployment()
        {
            var ct = new CancellationToken();
            var list = await _db.Events.Include(o => o.Details).Include(o => o.AssignedTo)
                .OrderBy(o => o.DisplayOrder).Where(x => x.AssessmentId == this.AssessmentId)
                .ToListAsync(ct);
            return View(list);
        }

        [HttpGet("exsum")]
        public ActionResult Exsum()
        {
            return View();
        }
    }
}