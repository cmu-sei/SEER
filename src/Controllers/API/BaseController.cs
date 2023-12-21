// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Linq;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Seer.Hubs;
using Seer.Infrastructure.Data;

namespace Seer.Controllers.API
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public class BaseController : Controller
    {
        protected ApplicationDbContext _db;
        protected IHubContext<ExecutionHub> _executionHubContext;
        
        internal string UserId => this.User.Claims.ToArray()[0].Value;
    }
    
    [Authorize(Roles = "EO, Admin")]
    [ApiController]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public class EOBaseController : Controller
    {
        protected ApplicationDbContext _db;
        protected IHubContext<ExecutionHub> _executionHubContext;
        
        internal string UserId => this.User.Claims.ToArray()[0].Value;
    }
}