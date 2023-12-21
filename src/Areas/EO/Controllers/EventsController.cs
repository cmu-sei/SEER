// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.ViewModels;

namespace Seer.Areas.EO.Controllers
{
    [Area("EO")]
    [Authorize(Roles = "EO,Admin")]
    [Route("eo/[controller]/")]
    public class EventsController : BaseController
    {
        public EventsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector)
        {
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var events = await _db.Events.Where(x => x.AssessmentId == this.AssessmentId).OrderBy(x => x.DisplayOrder).ToListAsync();
            return View(events);
        }

        [HttpGet("detail/{eventId}")]
        public async Task<ActionResult> Detail(int eventId)
        {
            var formattedEvent = new EventHistoryTableItemWithEventAndMet();
            formattedEvent.Event = await _db.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (formattedEvent.Event != null)
            {
                formattedEvent.METs = await _db.METs.Include(x => x.METItems)
                    .ThenInclude(x => x.METSCTs).ThenInclude(x => x.Score)
                    .Where(x => x.AssessmentId == this.AssessmentId).ToListAsync();

                formattedEvent.Histories = await _db.EventDetailHistory
                .Where(x => x.EventId == eventId
                            && x.HistoryAction != "CREATE"
                            && !x.Message.ToLower().Contains("owner to")
                            && !x.Message.ToLower().Contains("update user")
                            && !x.Message.ToLower().Contains("update alert")
                            && !x.Message.ToLower().Contains("severity to")
                            && !x.Message.ToLower().Contains("case delete")
                            && !x.Message.ToLower().Contains("status to cancel")
                )
                .Select(x => new EventHistoryTableItem
                {
                    Id = x.Id,
                    Created = x.Created,
                    Message = x.Message,
                    AssessmentId = x.AssessmentId,
                    EventId = x.EventId,
                    Status = x.Status,
                    Tags = x.Tags
                }).OrderByDescending(x => x.EventId).ThenBy(x => x.Id)
                .ToListAsync();
            }

            return View(formattedEvent);
        }

        [HttpPost("detail/{eventId}")]
        public async Task<ActionResult> Update(EventScoreUpdate model)
        {
            //pull up event
            var assessmentEvent = await _db.Events.FirstOrDefaultAsync(x => x.Id == model.EventId);
            if (assessmentEvent == null) return Redirect($"detail/{model.EventId}");

            //update numbers
            assessmentEvent.ScoreDiscovery = model.ScoreDiscovery;
            assessmentEvent.ScoreIntelligence = model.ScoreIntelligence;
            assessmentEvent.ScoreRemoval = model.ScoreRemoval;
            assessmentEvent.ScoreSeverity = model.ScoreSeverity;

            _db.Entry(assessmentEvent).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Redirect($"{model.EventId}");
        }
    }
}