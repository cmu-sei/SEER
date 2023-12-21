// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Seer.Hubs;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.Services;

namespace Seer.Controllers.API;

[AllowAnonymous]
[Route("/api/[controller]/")]
public class IntegrationController : BaseController
{
    private UserManager<User> _userManager;
    
    public IntegrationController(ApplicationDbContext dbContext, IHubContext<ExecutionHub> executionHubContext, UserManager<User> userManager)
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
            var message = new IntegrationMessage(payload);
            await message.Process(this._db, this._executionHubContext, this._userManager);
            return Ok("OK");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}