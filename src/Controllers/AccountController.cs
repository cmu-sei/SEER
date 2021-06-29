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
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;
using User = Seer.Infrastructure.Models.User;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AccountController : BaseController
    {
        private readonly SignInManager<User> _signInManager;

        public AccountController(SignInManager<User> signInManager,
            ApplicationDbContext dbContext, IDataProtectionProvider protector) :
            base(dbContext, protector)
        {
            _signInManager = signInManager;
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            foreach (var cookieKey in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookieKey);
            }

            if (!string.IsNullOrEmpty(returnUrl) &&
                returnUrl.StartsWith("/api", StringComparison.InvariantCultureIgnoreCase))
            {
                return Redirect("/api/error/403");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost("logoff")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOff()
        {
            await Cleanup();
            return Json(new { url = this.Url.Content("~/") });
        }

        [HttpGet("/account/logoff")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOut()
        {
            await Cleanup();
            return Redirect($"/account/login?e={Request.Query["e"]}&m={Request.Query["m"]}");
        }

        [Authorize]
        [HttpGet("sso")]
        public IActionResult Sso()
        {
            return Redirect("/home");
        }

        private async Task Cleanup()
        {
            if (!string.IsNullOrEmpty(this.UserId))
            {
                await _db.History.AddAsync(new History.HistoryItem
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