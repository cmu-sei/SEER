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