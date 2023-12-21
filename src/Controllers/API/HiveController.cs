// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Net;
using System.Threading.Tasks;
using Seer.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.Services;

namespace Seer.Controllers.API
{
    [AllowAnonymous]
    [Route("/api/[controller]/")]
    public class HiveController : BaseController
    {
        private UserManager<User> _userManager;
        public HiveController(ApplicationDbContext dbContext, IHubContext<ExecutionHub> executionHubContext, UserManager<User> userManager)
        {
            this._executionHubContext = executionHubContext;
            this._db = dbContext;
            this._userManager = userManager;
        }

        [HttpPost("webhook")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Webhook(object payload)
        {
            try
            {
                //var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
                await IntegrationMessageService.Process(this._db, this._executionHubContext, this._userManager, payload);
                return Ok("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}