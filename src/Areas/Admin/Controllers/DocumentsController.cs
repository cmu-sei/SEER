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

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class DocumentsController : BaseController
    {
        public DocumentsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }
        
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if(TempData["ErrorDocumentUpload"] != null)
            {
                ModelState.AddModelError(string.Empty, TempData["ErrorDocumentUpload"].ToString() ?? string.Empty);
            }
            return View(await _db.Documents.Where(o => o.AssessmentId == this.AssessmentId).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int id)
        {
            var document = await _db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Document document)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(document).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(document);
        }

        [HttpGet("{id}/delete")]
        public async Task<ActionResult> Delete(int id)
        {
            var document = await _db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View("Index");
        }

        [HttpPost("{id}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var document = await _db.Documents.FindAsync(id);
            _db.Documents.Remove(document);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        
        [HttpPost("savefile")]
        public async Task<IActionResult> SaveFile([FromForm]FileUpload model)  
        {
            try
            {
                if (model.File.Length > 0)
                {
                    var path = Directory.GetCurrentDirectory();
                    var fileName = model.File.FileName;

                    switch (model.Type.ToLower())
                    {
                        case "public":
                            path = Path.Combine(path, "wwwroot", "content", "documents", "2018", this.AssessmentId.ToString());
                            break;
                        case "private":
                            path = Path.Combine(path, "App_Data", "uploads", this.AssessmentId.ToString());
                            break;
                        case "inbrief":
                            if (!model.File.FileName.ToLower().EndsWith(".pdf"))
                                throw new ArgumentException($"{model.Type} must be a pdf");

                            path = Path.Combine(path, "wwwroot", "content", "documents", "2018", this.AssessmentId.ToString());
                            fileName = "GCD-2018-inbrief.pdf";
                            break;
                        case "map":
                            if (!model.File.FileName.ToLower().EndsWith(".pdf"))
                                throw new ArgumentException($"{model.Type} must be a pdf");

                            path = Path.Combine(path, "wwwroot", "content", "documents", "2018", this.AssessmentId.ToString());
                            fileName = "GCD-2018-map.pdf";
                            break;
                        default:
                            throw new ArgumentException("Application file type not set");
                    }

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path = Path.Combine(path, fileName);

                    await using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.File.CopyToAsync(stream);
                    }

                    var doc = new Document
                    {
                        Location = path, UserId = Guid.Parse(this.UserId), Name = fileName,
                        AssessmentId = this.AssessmentId.Value, Description = model.Description
                    };
                    await _db.Documents.AddAsync(doc);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                TempData["ErrorDocumentUpload"] = e.Message;
            }

            return RedirectToAction("Index");
        }  
        
        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)  
        {  
            var doc = _db.Documents.FirstOrDefault(o => o.Id == id);
            if (doc == null)
                return NotFound(); //upload truly not found
  
            var memory = new MemoryStream();
            await using (var stream = new FileStream(doc.Location, FileMode.Open))  
            {  
                await stream.CopyToAsync(memory);  
            }  
            memory.Position = 0;  
            
            return File(memory, doc.Location.GetContentType(), Path.GetFileName(doc.Location));  
        }

        public class FileUpload
        {
            public IFormFile File { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }
    }
}
