﻿/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class UploadsController : BaseController
    {
        public UploadsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index()
        {
            if (!this.AssessmentId.HasValue)
                return RedirectToAction("Assessment", "Home");

            var model = new List<Upload>();
            var aid = this.AssessmentId.Value;

            var a = _db.Assessments
                .Include(x => x.Uploads)
                .FirstOrDefault(o => o.Id == aid);

            if (a == null) return View(model);

            foreach (var upload in a.Uploads)
            {
                var u = _db.Users.FirstOrDefault(x => x.Id == upload.UserId);
                if (u != null) upload.UserId = $"{u.Name()} ({u.Email})";
                model.Add(upload);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "uploads",
                this.AssessmentId.ToString());
            var filePath = Path.Combine(folderPath, file.FileName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var doc = new Upload { Location = filePath, UserId = this.UserId, Name = file.FileName, AssessmentId = this.AssessmentId.Value };
            await _db.Uploads.AddAsync(doc);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var doc = _db.Uploads.FirstOrDefault(o => o.Id == id);
            if (doc == null)
                return NotFound();

            var memory = new MemoryStream();
            await using (var stream = new FileStream(doc.Location, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, doc.Location.GetContentType(), Path.GetFileName(doc.Location));
        }
    }
}
