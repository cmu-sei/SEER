// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FileHelpers;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seer.Infrastructure.Data;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class UsersController : BaseController
    {
        public UsersController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost("search")]
        public ActionResult Search(string q)
        {
            q = q.ToLower();

            var users = _db.Users
                .Where(o => o.FirstName.ToLower().Contains(q)
                            || o.LastName.ToLower().Contains(q)
                            || o.Email.ToLower().Contains(q)
                            || o.UserName.ToLower().Contains(q)
                            || o.OAuthId.ToLower().Contains(q)
                            || o.Id.ToLower().Contains(q)
                ).OrderBy(o => o.LastName).ToList();

            var results = users.Select(user => new SearchResults
            {
                Created = user.Created.ToString(CultureInfo.InvariantCulture),
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Username = user.UserName,
                Id = user.Id
            })
                .ToList();
            return Json(results);
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(User user)
        {
            if (!ModelState.IsValid) return View(user);

            var userManager = this.HttpContext.RequestServices.GetService<UserManager<User>>();
            await CreateUser(userManager, user);
            return RedirectToAction("Index", new { area = "Admin" });
        }

        [HttpGet("{id}")]
        public ActionResult Edit(string id)
        {
            if (id == null) return new BadRequestResult();

            var user = _db.Users.Find(id);
            if (user == null) return NotFound();

            var model = new UserViewModel(user);
            foreach (var g in _db.GroupUsers
                .Where(o => o.UserId == model.Id).ToList()
                .Select(ug => _db.Groups
                    .FirstOrDefault(o => o.Id == ug.GroupId)))
            {
                model.Groups.Add(g);
            }

            return View(model);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(User user)
        {
            if (!ModelState.IsValid) return View("Index");

            var dbUser = await _db.Users.FindAsync(user.Id);
            dbUser.FirstName = user.FirstName;
            dbUser.LastName = user.LastName;
            dbUser.Rank = user.Rank;
            dbUser.DutyPosition = user.DutyPosition;
            dbUser.UserName = user.UserName;
            dbUser.Email = user.Email;
            _db.Entry(dbUser).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", new { area = "Admin" });
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
                    var engine = new FileHelperEngine<UserImport>();
                    var imports = engine.ReadString(raw);
                    var userManager = this.HttpContext.RequestServices.GetService<UserManager<User>>();

                    try
                    {
                        foreach (var import in imports)
                        {
                            var o = new User(import);

                            if (_db.Users.Any(
                                x => x.UserName == o.UserName
                                     || x.Email == o.Email
                                     || !string.IsNullOrEmpty(x.OAuthId) && x.OAuthId == o.OAuthId))
                            {
                                continue;
                            }

                            await CreateUser(userManager, o);
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

        private async Task CreateUser(UserManager<User> userManager, User user)
        {
            await userManager.CreateAsync(user, "Scotty@1");
            await userManager.AddToRoleAsync(userManager.FindByEmailAsync(user.UserName).Result, Infrastructure.Models.User.Role.Player.ToString());
            await _db.SaveChangesAsync();
        }

        private class SearchResults
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Username { get; set; }
            public string Created { get; set; }
        }
    }
}