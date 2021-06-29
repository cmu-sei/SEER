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
                    await this._db.CatalogEvents.AddAsync(o);
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
            await this._db.CatalogEventDetails.AddAsync(item);
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