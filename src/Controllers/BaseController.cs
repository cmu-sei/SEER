// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Security.Claims;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using NLog;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Services;

namespace Seer.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BaseController : Controller
    {
        private readonly IDataProtector _protector;
        protected readonly ApplicationDbContext _db;
        internal static readonly Logger _log = LogManager.GetCurrentClassLogger();
        internal Configuration _configuration;

        internal BaseController(ApplicationDbContext dbContext, IDataProtectionProvider protector)
        {
            this._db = dbContext;
            this._protector = protector.CreateProtector("Seer.CookieProtector");

            this._configuration = ConfigurationService.Load();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.AssessmentConfiguration = this.AssessmentConfiguration;
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

        internal Assessment.Config AssessmentConfiguration
        {
            get
            {
                var o = new Assessment.Config
                {
                    HasAnnouncements = true,
                    HasDocuments = true,
                    HasFaqs = true,
                    HasIntel = true,
                    HasOrders = true,
                    HasUploads = true
                };

                try
                {
                    return string.IsNullOrEmpty(this.CookieRead("a.conf")) ?
                        o : JsonConvert.DeserializeObject<Assessment.Config>(this.CookieRead("a.conf"));
                }
                catch (Exception e)
                {
                    _log.Trace(e);
                    return o;
                }
            }
            set => this.CookieWrite("a.conf", JsonConvert.SerializeObject(value));
        }

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
