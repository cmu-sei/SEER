/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.Services;

namespace Seer.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class TimeController : BaseController
    {
        public TimeController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            int assessmentId;
            if (User.Identity is { IsAuthenticated: true })
            {
                if (!this.AssessmentId.HasValue)
                    return RedirectToAction("Assessment", "Home");

                assessmentId = this.AssessmentId.Value;
            }
            else
            {
                //have to figure out the currently active assessment
                var activeAssessmentTimeRecords = this._db.AssessmentTime.FirstOrDefault(x => x.Status == AssessmentTime.ExerciseTimeStatus.Active);
                if (activeAssessmentTimeRecords == null)
                {
                    ViewBag.Message = "No active assessment";
                    return View();
                }
                
                var activeAssessment = this._db.Assessments.FirstOrDefault(x => x.Id == activeAssessmentTimeRecords.AssessmentId);
                var group = this._db.Groups.FirstOrDefault(x => x.Id == activeAssessment.GroupId);
                
                if(activeAssessment == null || group == null)
                {
                    ViewBag.Message = "No active assessment";
                    return View();
                }
                
                assessmentId = activeAssessment.Id;
                
                ViewBag.AssessmentId = activeAssessment.Id;
                ViewBag.AssessmentName = activeAssessment.Name;
                ViewBag.GroupId = group.Id;
                ViewBag.GroupName = group.Name;
                
                this.AssessmentId = activeAssessment.Id;
                this.AssessmentName = activeAssessment.Name;
                this.GroupId = group.Id;
                this.GroupName = group.Name;
            }

            var assessmentTimeService = new AssessmentTimeService(this._db);
            await assessmentTimeService.Get(assessmentId);
            var t = assessmentTimeService.Time;

            return View(t);
        }
    }
}