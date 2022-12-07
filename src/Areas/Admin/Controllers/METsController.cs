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
using Seer.Areas.EO.ViewModels;
using System;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
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
    public class METsController : BaseController
    {
        public METsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = new EODashboardViewModel
            {
                METs = await _db.METs
                    .Include(x => x.METItems)
                    .ThenInclude(x => x.METSCTs)
                    .Where(o => o.AssessmentId == this.AssessmentId)
                    .ToListAsync(),
                Sections = await _db.Sections.Where(o => o.AssessmentId == this.AssessmentId).ToListAsync()
            };

            return View(model);
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MET model)
        {
            model.AssessmentId = this.AssessmentId.Value;
            if (!ModelState.IsValid) return View(model);

            _db.METs.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("edit")]
        public async Task<ActionResult> Edit()
        {
            var met = await _db.METs.Include(x => x.METItems).ThenInclude(x => x.METSCTs).FirstOrDefaultAsync(o => o.AssessmentId == this.AssessmentId);

            var scores = _db.METItemSCTScores.Where(o => o.METId == met.Id);

            if (met == null) return View((MET)null);

            foreach (var metItem in met.METItems)
            {
                foreach (var sct in metItem.METSCTs)
                {
                    foreach (var score in scores)
                    {
                        if (score.SCTId == sct.Id)
                        {
                            sct.Score = score;
                        }
                    }
                }
            }

            foreach (var metItem in met.METItems.ToList())
            {
                foreach (var sct in metItem.METSCTs.ToList())
                {
                    if (sct.SectionId > 0 && sct.SectionId != Convert.ToInt32(Request.Query["sectionid"]))
                    {
                        metItem.METSCTs.Remove(sct);
                    }
                }
            }

            return View(met);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MET model)
        {
            model.AssessmentId = this.AssessmentId.Value;
            if (!ModelState.IsValid) return View(model);

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("copy")]
        public async Task<ActionResult> Copy([FromQuery(Name = "from")] int from, [FromQuery(Name = "to")] int to)
        {
            var assessmentFrom = await _db.Assessments
                .Include(x => x.METs)
                .ThenInclude(x => x.METItems)
                .ThenInclude(x => x.METSCTs)
                .FirstOrDefaultAsync(o => o.Id == from);

            var assessmentTo = await _db.Assessments
                .Include(x => x.METs)
                .ThenInclude(x => x.METItems)
                .ThenInclude(x => x.METSCTs)
                .FirstOrDefaultAsync(o => o.Id == to);

            if (assessmentFrom == null || assessmentTo == null)
                return NotFound();

            if (assessmentTo.METs.Any())
                return Json("Assessment already has METs");

            foreach (var met in assessmentFrom.METs)
            {
                var newMet = new MET { AssessmentId = assessmentTo.Id, Name = met.Name, Created = DateTime.UtcNow };
                foreach (var metItem in met.METItems)
                {
                    var newMetItem = new METItem { Index = metItem.Index, Created = DateTime.UtcNow, Name = metItem.Name };

                    foreach (var sct in metItem.METSCTs)
                    {
                        var newSctItem = new METItemSCT
                        {
                            Created = DateTime.UtcNow,
                            Index = sct.Index,
                            Name = sct.Name,
                            SectionId = sct.SectionId,
                            Status = sct.Status
                        };

                        newMetItem.METSCTs.Add(newSctItem);
                    }

                    newMet.METItems.Add(newMetItem);
                }

                _db.METs.Add(newMet);
                await _db.SaveChangesAsync();
            }

            return Json("OK");
        }
    }
}
