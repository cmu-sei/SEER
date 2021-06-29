/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Threading.Tasks;
using System.Linq;
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
    public class AssessmentsController : BaseController
    {
        public AssessmentsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }

            var assessment = await _db.Assessments.FindAsync(id);
            if (assessment == null)
            {
                return NotFound();
            }

            this.AssessmentId = assessment.Id;
            this.AssessmentName = assessment.Name;
            return View(assessment);
        }

        [HttpGet("{id}/set")]
        public async Task<ActionResult> Set(int id, [FromQuery(Name = "g")] string goToUri)
        {
            var a = await this._db.Assessments.FirstOrDefaultAsync(o => o.Id == id);

            if (a == null)
            {
                return NotFound();
            }

            this.AssessmentId = a.Id;
            this.AssessmentName = a.Name;

            var g = this._db.Groups.FirstOrDefault(o => o.Id == a.GroupId);
            if (g != null)
            {
                this.GroupId = g.Id;
                this.GroupName = g.Name;
            }

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return RedirectToAction("Edit", "Assessments", new { id, area = "Admin" });
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Assessment assessment)
        {
            if (!ModelState.IsValid) return View(assessment);

            await _db.Assessments.AddAsync(assessment);
            await _db.SaveChangesAsync();

            this.AssessmentId = assessment.Id;
            this.AssessmentName = assessment.Name;

            return RedirectToAction("Edit", "Assessments", new { assessment.Id, area = "Admin" });

        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([FromForm] Assessment model)
        {
            var item = await this._db.Assessments.FindAsync(model.Id);
            item.Status = model.Status;
            item.Name = model.Name;
            item.Created = model.Created;

            if (!ModelState.IsValid) return View(item);

            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Edit", "Assessments", new { item.Id, area = "Admin" });
        }
    }
}
