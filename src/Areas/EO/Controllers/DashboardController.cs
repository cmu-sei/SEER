// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Areas.EO.Controllers
{
    [Area("EO")]
    [Authorize(Roles = "EO,Admin")]
    [Route("eo/[controller]")]
    public class DashboardController : BaseController
    {
        public DashboardController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector)
        {
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = new HomeViewModel();

            var gu = await this._db.GroupUsers.Where(o => o.UserId == this.UserId).ToArrayAsync();
            foreach (var g in gu)
            {
                var groups = await this._db.Groups
                    .Where(o => o.Id == g.GroupId && o.Status == ActiveStatus.Active)
                    .Include(x => x.Assessments)
                    .ToListAsync();
                foreach (var group in groups)
                {
                    model.Groups.Add(group);
                }
            }

            if (model.Groups.Count == 1)
            {
                if (model.Groups[0].Assessments.Count == 1)
                {
                    this.AssessmentId = model.Groups[0].Assessments[0].Id;
                    this.AssessmentName = model.Groups[0].Assessments[0].Name;
                    return RedirectToAction("Index", "METL", new { id = model.Groups[0].Assessments[0].Id });
                }
            }

            return View(model);
        }

        [HttpGet("set/{id}")]
        public async Task<ActionResult> Set(int id)
        {
            var gu = await this._db.GroupUsers.Where(o => o.UserId == this.UserId).ToArrayAsync();
            var isMember = false;
            foreach (var g in gu)
            {
                foreach (var group in await this._db.Groups
                    .Where(o => o.Id == g.GroupId && o.Status == ActiveStatus.Active).Include(x => x.Assessments)
                    .ToListAsync())
                {
                    foreach (var assessment in group.Assessments)
                    {
                        if (assessment.Id == id)
                        {
                            isMember = true;
                            this.GroupName = group.Name;
                        }
                    }
                }
            }

            if (!isMember)
                throw new UnauthorizedAccessException("You are not part of this assessment");

            var a = await this._db.Assessments.FirstOrDefaultAsync(o => o.Id == id);

            this.AssessmentId = a.Id;
            this.AssessmentName = a.Name;
            this.GroupId = a.GroupId;

            return RedirectToAction("Index", "Measure");
        }
    }
}