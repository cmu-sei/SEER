// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class TaskingOrdersController : BaseController
    {
        public TaskingOrdersController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await _db.TaskingItems.Where(o => o.AssessmentId == this.AssessmentId).OrderBy(o => o.Index).ToListAsync());
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create(TaskingItem model)
        {
            try
            {
                model.Created = DateTime.UtcNow;
                model.AssessmentId = this.AssessmentId.Value;
                _db.TaskingItems.Add(model);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int id)
        {
            var model = await _db.TaskingItems.FirstOrDefaultAsync(o => o.Id == id);
            return View(model);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> Edit(int id, TaskingItem model)
        {
            try
            {
                model.Id = id;
                model.Created = DateTime.UtcNow;
                model.AssessmentId = this.AssessmentId.Value;
                _db.TaskingItems.AddOrUpdate(model);
                await _db.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
