// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NLog;
using Seer.Infrastructure.Data;

namespace Seer.Hubs
{
    public class HubBase : Hub
    {
        internal static Logger _log = LogManager.GetCurrentClassLogger();

        internal ApplicationDbContext _context;

        public HubBase(ApplicationDbContext context)
        {
            this._context = context;
        }

        internal HttpContext GetHttpContext()
        {
            return Context.GetHttpContext();
        }

        internal string UserId => Context.User.Claims.ToArray()[0].Value;
        internal string UserName => Context.User.Identity.Name;
    }
}