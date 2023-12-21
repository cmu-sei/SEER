// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using NLog;
using Seer.Hubs;
using Seer.Infrastructure.Data;

namespace Seer.Areas.Admin.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]

    public class BaseController : Controller
    {
        private readonly IDataProtector _protector;
        protected ApplicationDbContext _db;
        protected IHubContext<ExecutionHub> _executionHubContext;
        internal static readonly Logger _log = LogManager.GetCurrentClassLogger();

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

        private void CookieWrite(string key, string value)
        {
            var protectedData = _protector.Protect(value);

            var option = new CookieOptions { Expires = DateTime.Now.AddMonths(1) };
            Response.Cookies.Append(key, protectedData, option);
        }

        private string CookieRead(string cookieName)
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
                var o = this.CookieRead("a.a.id");
                return !string.IsNullOrEmpty(o) ? Convert.ToInt32(o) : null;
            }
            set => this.CookieWrite("a.a.id", value.ToString());
        }

        internal string AssessmentName
        {
            get => this.CookieRead("a.a.name");
            set => this.CookieWrite("a.a.name", value);
        }

        internal string UserId => this.User.Claims.ToArray()[0].Value;

        internal int? GroupId
        {
            get => Convert.ToInt32(this.CookieRead("a.g.id"));
            set => this.CookieWrite("a.g.id", value.ToString());
        }

        protected string GroupName
        {
            get => this.CookieRead("a.g.name");
            set => this.CookieWrite("a.g.name", value);
        }
    }
}
