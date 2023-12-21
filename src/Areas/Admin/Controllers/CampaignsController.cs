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
    public class CampaignsController : BaseController
    {
        public CampaignsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var list = await _db.Campaigns.OrderByDescending(o => o.Id).ToArrayAsync();
            return View(list);
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Campaign c)
        {
            if (!ModelState.IsValid) return View();

            _db.Campaigns.Add(c);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int id)
        {
            var item = await _db.Campaigns.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Campaign c)
        {
            if (!ModelState.IsValid) return View(c);

            _db.Entry(c).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("{id}/delete")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            var item = await _db.Campaigns.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost("{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var c = await _db.Campaigns.FindAsync(id);
            _db.Campaigns.Remove(c);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}