/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Collections.Generic;
using System.Linq;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class IntelController : BaseController
    {
        public IntelController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index()
        {
            if (!this.AssessmentId.HasValue)
                return RedirectToAction("Assessment", "Home");

            return View();
        }

        [HttpPost("search")]
        public ActionResult Search(string q)
        {
            List<IntelItem> items;
            if (string.IsNullOrEmpty(q))
            {
                items = this._db.IntelItems.Where(o =>
                        o.AssessmentId == this.AssessmentId.Value && o.Status == ActiveStatus.Active)
                    .OrderByDescending(o => o.Created)
                    .ToList();

            }
            else
            {
                items = this._db.IntelItems
                    .Where(o => o.AssessmentId == this.AssessmentId.Value && o.Status == ActiveStatus.Active &&
                                (o.Details.Contains(q) ||
                                 o.Subject.Contains(q) ||
                                 o.Tags.Contains(q)))
                    .OrderByDescending(o => o.Created)
                    .ToList();
            }

            return Json(items);
        }

        [HttpGet("{id}")]
        public ActionResult Details(int id)
        {
            var intelItem = this._db.IntelItems.FirstOrDefault(o => o.AssessmentId == this.AssessmentId.Value && o.Status == ActiveStatus.Active && o.Id == id);
            if (intelItem == null)
            {
                return NotFound();
            }

            return View(intelItem);
        }
    }
}
