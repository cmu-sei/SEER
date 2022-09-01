/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

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
