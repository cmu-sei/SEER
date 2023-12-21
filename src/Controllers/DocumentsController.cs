// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class DocumentsController : BaseController
    {
        public DocumentsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index()
        {
            if (!this.AssessmentId.HasValue)
                return RedirectToAction("Assessment", "Home");

            var docs = this._db.Documents.Where(o => o.AssessmentId == this.AssessmentId).ToList();

            return View(docs);
        }

        [HttpGet("{id:int}")]
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
    }
}
