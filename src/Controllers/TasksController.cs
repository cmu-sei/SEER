// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Seer.Infrastructure.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class TasksController : BaseController
    {
        public TasksController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index()
        {
            if (!this.AssessmentId.HasValue)
                return RedirectToAction("Assessment", "Home");

            var aid = this.AssessmentId.Value;
            var model = new TaskingViewModel();

            var tasks = _db.TaskingItems.Include(o => o.TaskingItemDocuments).Where(o => o.AssessmentId == aid && o.Status == ActiveStatus.Active).ToList();
            model.Items = tasks;

            foreach (var item in model.Items)
            {
                var r = _db.TaskingItemResults.Where(o => o.TaskingItemId == item.Id);
                foreach (var res in r)
                {
                    model.Results.Add(res);
                }
            }

            model.GroupId = this.GroupId.Value;
            return View(model);
        }
    }
}
