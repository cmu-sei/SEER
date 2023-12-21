// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Seer.Hubs;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Services;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class AssessmentTimesController : BaseController
    {
        private readonly IHubContext<AssessmentTimeHub> _hubContext;

        public AssessmentTimesController(ApplicationDbContext dbContext, IDataProtectionProvider protector,
            IHubContext<AssessmentTimeHub> hubContext) : base(dbContext, protector)
        {
            this._hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var assessmentTimeService = new AssessmentTimeService(this._db, this._hubContext);
            await assessmentTimeService.Get(this.AssessmentId.Value);

            var item = assessmentTimeService.Time;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(AssessmentTime model)
        {
            model.AssessmentId = this.AssessmentId.Value;
            var assessmentTimeService = new AssessmentTimeService(this._db, this._hubContext) { Time = model };
            await assessmentTimeService.Set();
            return RedirectToAction("Index", "AssessmentTimes", new { area = "Admin" });
        }
    }
}
