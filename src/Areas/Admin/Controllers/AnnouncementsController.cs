// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
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
    public class AnnouncementsController : BaseController
    {
        public AnnouncementsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await _db.Announcements.Where(o => o.AssessmentId == this.AssessmentId).ToListAsync());
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Announcement announcement)
        {
            if (!this.AssessmentId.HasValue)
                throw new Exception("No assessment id found");
            announcement.AssessmentId = this.AssessmentId.Value;
            if (!ModelState.IsValid) return View(announcement);

            _db.Announcements.Add(announcement);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int id)
        {
            var announcement = await _db.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Announcement announcement)
        {
            if (!this.AssessmentId.HasValue)
                throw new Exception("No assessment id found");
            announcement.AssessmentId = this.AssessmentId.Value;
            if (!ModelState.IsValid) return View(announcement);

            _db.Entry(announcement).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
