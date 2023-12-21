// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
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

        [HttpGet("{id:int}")]
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

            _db.Assessments.Add(assessment);
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
            if(item == null)
                throw new Exception("No assessment id found");
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
