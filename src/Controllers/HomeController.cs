/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

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

            var userJwt = _db.UserJwts.OrderByDescending(x => x.Id).FirstOrDefault(x => x.SsoId == this.UserId);
            if (userJwt != null)
            {
                ViewBag.Token = userJwt.Jwt;
            }

            return View(model);

            //not logged in? go do that
        }

        [Authorize]
        [HttpGet("assessment")]
        public IActionResult Assessment()
        {
            if (!this.AssessmentId.HasValue) return RedirectToAction("Index", "Home");
            if (User.Identity == null || !User.Identity.IsAuthenticated) return View();

            var model = new OpViewModel { Quizzes = new List<Quiz>() };
            var a = this._db.Assessments.Include(o => o.Quizzes).FirstOrDefault(o => o.Id == this.AssessmentId);
            if (a != null)
                model.Quizzes = a.Quizzes.ToList();
            if (model.Quizzes.Count > 0)
                model.Quizzes = model.Quizzes.OrderBy(o => o.Index).ToList();
            return View(model);
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
