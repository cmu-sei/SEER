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
        private IDataProtector _protector;
        protected ApplicationDbContext _db;
        internal static Logger _log = LogManager.GetCurrentClassLogger();
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
                return !string.IsNullOrEmpty(o) ? (int?)Convert.ToInt32(o) : null;
            }
            set => this.CookieWrite("a.id", value.ToString());
        }

        internal string AssessmentName
        {
            get => this.CookieRead("a.name");
            set => this.CookieWrite("a.name", value);
        }

        internal string UserId => this.User.FindFirstValue("sub");

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
                    HasQuizzes = true,
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
