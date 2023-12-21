// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using Seer.Infrastructure.Data;

namespace Seer.Areas.EO.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]

    public class BaseController : Controller
    {
        private IDataProtector _protector;
        protected ApplicationDbContext _db;
        internal static Logger _log = LogManager.GetCurrentClassLogger();

        internal BaseController(ApplicationDbContext dbContext, IDataProtectionProvider protector)
        {
            this._db = dbContext;
            this._protector = protector.CreateProtector("Seer.CookieProtector");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.AssessmentId = this.AssessmentId;
            ViewBag.AssessmentName = this.AssessmentName;
            ViewBag.GroupId = this.GroupId;
            ViewBag.GroupName = this.GroupName;

            base.OnActionExecuting(context);
        }

        internal void CookieWrite(string key, string value)
        {
            var protectedData = _protector.Protect(value);

            var option = new CookieOptions { Expires = DateTime.Now.AddMonths(1) };
            Response.Cookies.Append(key, protectedData, option);
        }

        internal string CookieRead(string cookieName)
        {
            try
            {
                var c = Request.Cookies[cookieName];
                return c != null ? _protector.Unprotect(c) : null;
            }
            catch (Exception e)
            {
                _log.Debug(e);
                return null;
            }
        }

        internal int? AssessmentId
        {
            get
            {
                var o = this.CookieRead("a.id");
                return !string.IsNullOrEmpty(o) ? Convert.ToInt32(o) : null;
            }
            set => this.CookieWrite("a.id", value.ToString());
        }

        internal string AssessmentName
        {
            get => this.CookieRead("a.name");
            set => this.CookieWrite("a.name", value);
        }

        internal string UserId => this.User.Claims.ToArray()[0].Value;

        internal int? GroupId
        {
            get => Convert.ToInt32(this.CookieRead("g.id"));
            set => this.CookieWrite("g.id", value.ToString());
        }

        public string GroupName
        {
            get => this.CookieRead("g.name");
            set => this.CookieWrite("g.name", value);
        }
    }
}
