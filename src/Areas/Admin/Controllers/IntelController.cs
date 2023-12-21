// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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
    public class IntelController : BaseController
    {
        public IntelController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await _db.IntelItems.Where(o => o.AssessmentId == this.AssessmentId).ToListAsync());
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IntelItem intelItem)
        {
            if (ModelState.IsValid)
            {
                intelItem.AssessmentId = this.AssessmentId.Value;
                _db.IntelItems.Add(intelItem);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(intelItem);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int id)
        {
            var intelItem = await _db.IntelItems.FindAsync(id);
            if (intelItem == null)
            {
                return NotFound();
            }
            return View(intelItem);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IntelItem intelItem)
        {
            if (ModelState.IsValid)
            {
                intelItem.AssessmentId = this.AssessmentId.Value;
                _db.Entry(intelItem).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(intelItem);
        }

        [HttpGet("{id}/delete")]
        public async Task<ActionResult> Delete(int id)
        {
            var intelItem = await _db.IntelItems.FindAsync(id);
            if (intelItem == null)
            {
                return NotFound();
            }
            return View(intelItem);
        }

        [HttpPost("{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var intelItem = await _db.IntelItems.FindAsync(id);
            _db.IntelItems.Remove(intelItem);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
