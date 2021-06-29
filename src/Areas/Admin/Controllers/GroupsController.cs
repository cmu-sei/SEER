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
using System.Linq;
using System.Threading.Tasks;
using FileHelpers;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class GroupsController : BaseController
    {
        public GroupsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index()
        {
            return View(_db.Groups
                .OrderBy(o => o.SignalCorp).ThenBy(o => o.Theatre).ThenBy(o => o.Brigade).ThenBy(o => o.Name)
                .ToList());
        }

        [HttpGet("{id}/set")]
        public async Task<ActionResult> Set(int id)
        {
            var g = await _db.Groups.FirstOrDefaultAsync(o => o.Id == id);

            this.GroupId = g.Id;
            this.GroupName = g.Name;

            this.AssessmentId = -1;
            this.AssessmentName = "";

            return RedirectToAction("Edit", new { id, area = "Admin" });
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Group model)
        {
            if (!ModelState.IsValid) return View(model);

            await _db.Groups.AddAsync(model);
            await _db.SaveChangesAsync();
            _log.Debug($"Group created: {model.Name} by {User.Identity.Name}");

            this.GroupId = model.Id;
            this.GroupName = model.Name;

            return RedirectToAction("Index", new { area = "Admin" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            var group = await _db.Groups.Include(o => o.Assessments).Include(o => o.Users).FirstOrDefaultAsync(o => o.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            this.GroupId = group.Id;
            this.GroupName = group.Name;

            var assessments = group.Assessments;
            var users = group.Users;

            var model = new GroupViewModel(group) { Assessments = assessments };

            foreach (var user in users)
            {
                var u = await _db.Users.FirstOrDefaultAsync(o => o.Id == user.UserId);
                if (u != null)
                    model.Users.Add(u);
            }

            return View(model);
        }

        // POST: Admin/Groups/Edit/5
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(GroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(model.Group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                _log.Debug($"Group edited: {model.Group.Name} by {User.Identity.Name}");
                return RedirectToAction("Index", new { area = "Admin" });
            }

            model.Assessments = model.Assessments.OrderBy(o => o.Name).ToList();
            return View(model);
        }

        [HttpGet("import")]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost("import")]
        public async Task<ActionResult> Import([FromForm] IFormFile file)
        {
            try
            {
                if (file.Length > 0)
                {
                    var raw = file.ReadAsList();
                    var engine = new FileHelperEngine<GroupImport>();
                    var imports = engine.ReadString(raw);

                    try
                    {
                        foreach (var import in imports)
                        {
                            var group = new Group(import);
                            group.Status = ActiveStatus.Active;
                            _db.Groups.AddOrUpdate(group);
                            await _db.SaveChangesAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error($"{e.Message} : {e.StackTrace}");
                        ViewBag.MessageType = "danger";
                        ViewBag.Message = "File upload failed - please check your import template format!";
                        return View();
                    }
                }
                ViewBag.MessageType = "success";
                ViewBag.Message = "File Uploaded Successfully!";
            }
            catch (Exception e)
            {
                _log.Error($"{e.Message} : {e.StackTrace}");
                ViewBag.MessageType = "danger";
                ViewBag.Message = "File upload failed!";
            }
            return View();
        }
    }
}