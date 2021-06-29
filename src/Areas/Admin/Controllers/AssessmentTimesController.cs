/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

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
