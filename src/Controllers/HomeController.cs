// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Controllers
{
    
    public class HomeController : BaseController
    {
        public HomeController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index()
        {
            if (User.Identity is not { IsAuthenticated: true })
            {
                return RedirectToAction("Index", "Time");
            }

            var model = new HomeViewModel();

            var gu = this._db.GroupUsers.Where(o => o.UserId == this.UserId).ToArray();
            foreach (var g in gu)
            {
                foreach (var group in this._db.Groups
                    .Where(o => o.Id == g.GroupId && o.Status == ActiveStatus.Active)
                    .Include(x => x.Assessments)
                    .ToList())
                {
                    model.Groups.Add(@group);
                }

                model.Groups = model.Groups.OrderBy(o => o.Name).Distinct().ToList();
            }

            if (model.Groups.Count == 1)
            {
                if (model.Groups[0].Assessments.Count == 1)
                {
                    this.AssessmentId = model.Groups[0].Assessments[0].Id;
                    this.AssessmentName = model.Groups[0].Assessments[0].Name;
                    this.AssessmentConfiguration = new Assessment.Config(model.Groups[0].Assessments[0]);

                    return RedirectToAction("Assessment", this.AssessmentId);
                }
            }
            
            return View(model);
        }

        [Authorize]
        [HttpGet("assessment")]
        public IActionResult Assessment()
        {
            return View();
        }

        [Authorize]
        [HttpGet("setassessment")]
        public ActionResult SetAssessment(int id)
        {
            var gu = this._db.GroupUsers.Where(o => o.UserId == this.UserId).ToArray();
            var isMember = false;
            int? gid = null;
            string groupName = null;
            foreach (var g in gu)
            {
                foreach (var group in this._db.Groups
                    .Where(o => o.Id == g.GroupId && o.Status == ActiveStatus.Active).Include(x => x.Assessments)
                    .ToList())
                {
                    foreach (var assessment in group.Assessments)
                    {
                        if (assessment.Id != id) continue;

                        gid = @group.Id;
                        groupName = @group.Name;
                        isMember = true;
                    }
                }
            }

            this.GroupId = gid;
            this.GroupName = groupName;

            if (!isMember)
                throw new UnauthorizedAccessException("You are not part of this assessment");

            var a = this._db.Assessments.FirstOrDefault(o => o.Id == id);

            if (a == null) return RedirectToAction("Assessment");

            this.AssessmentId = a.Id;
            this.AssessmentName = a.Name;

            var conf = new Assessment.Config(a);
            this.AssessmentConfiguration = conf;

            return RedirectToAction("Assessment");
        }
    }
}
