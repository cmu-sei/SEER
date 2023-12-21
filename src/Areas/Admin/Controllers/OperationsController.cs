// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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
    public class OperationsController : BaseController
    {
        public OperationsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet("/admin/campaigns/{campaignId}/operations")]
        public async Task<ActionResult> Index(int campaignId)
        {
            var item = await _db.Campaigns.Include(o => o.Operations).FirstOrDefaultAsync(o => o.Id == campaignId);
            return View(item.Operations);

        }

        [HttpGet("/admin/campaigns/{campaignId}/operations/create")]
        public ActionResult Create(int campaignId)
        {
            return View();
        }

        [HttpPost("/admin/campaigns/{campaignId}/operations/create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int campaignId, Operation o)
        {
            var item = await _db.Campaigns.Include(x => x.Operations).FirstOrDefaultAsync(x => x.Id == campaignId);
            item.Operations.Add(o);

            if (!ModelState.IsValid) return View();

            _db.Entry(item).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("/admin/campaigns/{campaignId}/operations/{operationId}")]
        public async Task<ActionResult> Edit(int campaignId, int operationId)
        {
            var item = await _db.Operations.FindAsync(operationId);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost("/admin/campaigns/{campaignId}/operations/{operationId}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int campaignId, int operationId, Operation o)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(o).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(o);
        }

        [HttpGet("/admin/campaigns/{campaignId}/operations/{operationId}/delete")]
        public async Task<ActionResult> Delete(int campaignId, int operationId)
        {
            var item = await _db.Operations.FindAsync(operationId);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost("/admin/campaigns/{campaignId}/operations/{operationId}/delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int campaignId, int operationId)
        {
            var c = await _db.Operations.FindAsync(operationId);
            _db.Operations.Remove(c);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}