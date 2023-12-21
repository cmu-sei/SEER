// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Seer.Areas.Admin.ViewModels;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using YamlDotNet.Serialization;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class CatalogController : BaseController
    {
        public CatalogController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var o = new CatalogViewModel
            {
                CatalogItems = await this._db.CatalogEvents
                    .Include(x => x.Details)
                    .ToArrayAsync()
            };
            return View(o);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await this._db.CatalogEvents
                .Include(o => o.Details)
                .FirstOrDefaultAsync(x => x.Id == id);
            return View(item);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            var o = new AssessmentEventCatalogItem();
            return View("Edit", o);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> Edit([FromForm] AssessmentEventCatalogItem item)
        {
            this._db.CatalogEvents.Update(item);
            await this._db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("import")]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost("import")]
        public async Task<ActionResult> Import([FromForm] List<IFormFile> files)
        {
            try
            {
                foreach (var file in files)
                {
                    if (file.Length <= 0) continue;

                    var raw = file.ReadAsList();
                    var deserializer = new DeserializerBuilder().Build();
                    var o = deserializer.Deserialize<AssessmentEventCatalogItem>(raw);
                    this._db.CatalogEvents.Add(o);
                    await this._db.SaveChangesAsync();
                }

                ViewBag.MessageType = "success";
                ViewBag.Message = "Files Uploaded Successfully!";
                return Redirect("/admin/catalog");
            }
            catch (Exception e)
            {
                _log.Error($"{e.Message} : {e.StackTrace}");
                ViewBag.MessageType = "danger";
                ViewBag.Message = "File upload failed!";
                return View();
            }
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var o = await _db.CatalogEvents.FindAsync(id);
            _db.CatalogEvents.Remove(o);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // ==== event details =====

        [HttpPost("{id}/details")]
        public async Task<ActionResult> CreateEvent(int id, [FromForm] AssessmentEventCatalogItemDetail item, [FromQuery(Name = "g")] string goToUri)
        {
            item.AssessmentEventCatalogItemId = id;
            this._db.CatalogEventDetails.Add(item);
            await this._db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return RedirectToAction("Index");
        }

        [HttpPost("{id}/details/{detailId}")]
        public async Task<ActionResult> EditEvent(int id, int detailId, [FromForm] AssessmentEventCatalogItemDetail item, [FromQuery(Name = "g")] string goToUri)
        {
            this._db.CatalogEventDetails.Update(item);
            await this._db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return RedirectToAction("Index");
        }

        [HttpPost("{id}/details/{detailId}/delete")]
        public async Task<IActionResult> DeleteEvent(int id, int detailId, [FromQuery(Name = "g")] string goToUri)
        {
            var o = await _db.CatalogEventDetails.FindAsync(detailId);
            _db.CatalogEventDetails.Remove(o);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return RedirectToAction("Index");
        }
    }
}