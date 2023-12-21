// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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

        [HttpGet("{id:int}")]
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
