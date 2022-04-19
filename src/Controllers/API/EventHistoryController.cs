/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

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