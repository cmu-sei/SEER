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
                await _db.IntelItems.AddAsync(intelItem);
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
