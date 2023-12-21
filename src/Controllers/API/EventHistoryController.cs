// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Seer.Hubs;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Services;
using Seer.Infrastructure.ViewModels;

namespace Seer.Controllers.API
{
    [Route("/api/[controller]/")]
    public class EventHistoryController : BaseController
    {
        public EventHistoryController(ApplicationDbContext dbContext, IHubContext<ExecutionHub> executionHubContext)
        {
            this._db = dbContext;
            this._executionHubContext = executionHubContext;
        }

        [HttpPost("status")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Status(EventHistoryStatusUpdate statusUpdate, CancellationToken ct)
        {
            return Ok(await IntegrationMessageService.UpdateStatus(this._db, statusUpdate, ct));
        }

        [HttpPost("associate")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Associate(EventHistoryAssociation association, CancellationToken ct)
        {
            return Ok(await IntegrationMessageService.Associate(this._db, association, ct));
        }

        [HttpGet("match")]
        public async Task<IActionResult> Match()
        {
            var count = await IntegrationMessageService.TryMatchUnmatched(this._db);
            return Ok(count);
        }

        [HttpGet("fix")]
        public async Task<IActionResult> Fix()
        {
            return Ok(await IntegrationMessageService.Fix(this._db, this._executionHubContext));
        }
    }
}