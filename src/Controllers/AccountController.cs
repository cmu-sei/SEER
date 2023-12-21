// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;
using User = Seer.Infrastructure.Models.User;

namespace Seer.Controllers
{
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    [Route("[controller]")]
    public class AccountController : BaseController
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _db;

        public AccountController(SignInManager<User> signInManager, ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector)
        {
            _signInManager = signInManager;
            _db = dbContext;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost("logoff")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOff()
        {
            await Cleanup();
            return Redirect($"/account/login?e={Request.Query["e"]}&m={Request.Query["m"]}");
        }

        [HttpGet("/account/logoff")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOut()
        {
            await Cleanup();
            return Redirect($"/account/login?e={Request.Query["e"]}&m={Request.Query["m"]}");
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            foreach (var cookieKey in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookieKey);
            }

            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/api", StringComparison.InvariantCultureIgnoreCase))
            {
                return Redirect("/api/error/403");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, [FromQuery(Name = "ReturnUrl")] string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "/assessment";

            // Require the user to have a confirmed email before they can log on.
            var user = await this._signInManager.UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                await Register(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result =
                await this._signInManager.PasswordSignInAsync(model.Email, model.Password, true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Redirect(returnUrl);
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }
        
        private async Task Register(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User() { UserName = model.Email, Email = model.Email };
                var result = await this._signInManager.UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _log.Info($"Register user for {user.Email}");
                }
                else
                {
                    _log.Error($"Register failed with {string.Concat(result.Errors, " ")}");
                    throw new Exception($"Registration failed due to {string.Concat(result.Errors, " ")}");
                }

                if (Program.Configuration.DefaultAdminAccounts.Any(user.Email.Split('@')[0].Contains))
                {
                    await this._signInManager.UserManager.AddToRoleAsync(user, Infrastructure.Models.User.Role.Admin.ToString());
                    _log.Debug($"Adding roles...");
                    await this._signInManager.UserManager.AddToRoleAsync(user, Infrastructure.Models.User.Role.Admin.ToString());
                    var groups = _db.Groups.Where(o => o.Status == ActiveStatus.Active).ToArray();
                    foreach (var group in groups)
                    {
                        _db.GroupUsers.Add(new GroupUser { GroupId = group.Id, UserId = user.Id });
                        _log.Trace($"adding user {user.Email} to group: {group.Name}");
            
                        await _db.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task Cleanup()
        {
            if (!string.IsNullOrEmpty(this.UserId))
            {
                _db.History.Add(new History.HistoryItem
                    { Key = History.HistoryItem.HistoryKey.LogOff, UserId = Guid.Parse(this.UserId) });
                await _db.SaveChangesAsync();
            }

            await _signInManager.SignOutAsync();

            var myCookies = Request.Cookies.Keys;
            foreach (var cookie in myCookies)
            {
                Response.Cookies.Delete(cookie);
            }
        }
    }
}