// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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