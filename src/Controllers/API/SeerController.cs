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
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Seer.Hubs;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Controllers.API
{
    [Route("api/seer/")]
    public class SeerController : BaseController
    {
        private IHubContext<ExecutionHub> _hubContext;

        public SeerController(ApplicationDbContext dbContext, IHubContext<ExecutionHub> hubcontext)
        {
            this._hubContext = hubcontext;
            this._db = dbContext;
        }

        [HttpGet("datapoints/{campaignId}")]
        //[ProducesResponseType(typeof(IEnumerable<QuizAnswer>), (int) HttpStatusCode.OK)]
        public async Task<IEnumerable<CampaignDataPoint>> DatapointsGet(int campaignId, CancellationToken ct)
        {
            var campaign = await this._db.Campaigns.FirstOrDefaultAsync(x => x.Id == campaignId, ct);
            return campaign.DataPoints;
        }

        [HttpPost("datapoints/{campaignId}")]
        //[ProducesResponseType(typeof(IEnumerable<QuizAnswer>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> DatapointsCreate(int campaignId, CampaignDataPoint dataPoint, CancellationToken ct)
        {
            var campaign = await this._db.Campaigns.FirstOrDefaultAsync(x => x.Id == campaignId, ct);
            campaign.DataPoints.Add(dataPoint);
            await this._db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpPost("datapoints/{campaignId}/{dataPointId}/delete")]
        //[ProducesResponseType(typeof(IEnumerable<QuizAnswer>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> DatapointsDelete(int campaignId, int dataPointId, CancellationToken ct)
        {
            var dataPoint = await this._db.CampaignDataPoints.FirstOrDefaultAsync(x => x.Id == dataPointId, ct);
            this._db.CampaignDataPoints.Remove(dataPoint);
            await this._db.SaveChangesAsync(ct);
            return Ok();
        }

        [HttpGet("assessments/{assessmentId}/events")]
        [ProducesResponseType(typeof(IEnumerable<Event>), (int)HttpStatusCode.OK)]
        public IEnumerable<Event> EventsGet(int assessmentId)
        {
            var o = this._db.Events.Include(x => x.Details).Where(x => x.AssessmentId == assessmentId).OrderBy(x => x.DisplayOrder);
            foreach (var evt in o)
            {
                foreach (var detail in evt.Details)
                {
                    if (!string.IsNullOrEmpty(detail.AssociatedSCTs))
                    {
                        foreach (var c in detail.AssociatedSCTs.Split(Convert.ToChar(",")))
                        {
                            var s = _db.METScts.FirstOrDefault(x => x.Id == Convert.ToInt32(c));
                            var scores = _db.METItemSCTScores.Where(x => x.METId == s.Id);
                            foreach (var score in scores.OrderByDescending(x => x.Created))
                            {
                                s.Score = score;
                                break;
                            }

                            detail.SCTs.Add(s);
                        }
                    }

                    if (string.IsNullOrEmpty(detail.AssociatedQuizQuestions)) continue;
                    {
                        foreach (var c in detail.AssociatedQuizQuestions.Split(Convert.ToChar(",")))
                        {
                            var s = _db.Questions.FirstOrDefault(x => x.Id == Convert.ToInt32(c));
                            var answers = _db.Answers.Where(x => x.QuestionId == s.Id);
                            foreach (var answer in answers.OrderByDescending(x => x.Created))
                            {
                                s.AnsweredIndex = answer.AnsweredIndex;
                                s.AnsweredBy = answer.UserId;
                                s.AnsweredText = answer.AnsweredText;
                                s.AnswerStatus = answer.Status;
                                break;
                            }
                            detail.QuizQuestions.Add(s);
                        }
                    }
                }
            }

            return o;
        }

        [HttpPost("assessments/{assessmentId}/events/{eventId}")]
        [ProducesResponseType(typeof(Task<Event>), (int)HttpStatusCode.OK)]
        public async Task<Event> EventsUpdate(int assessmentId, int eventId, Event evt)
        {
            this._db.Events.Update(evt);
            await this._db.SaveChangesAsync();
            return evt;
        }

        /// <summary>
        /// gets event history
        /// </summary>
        /// <param name="assessmentId"></param>
        /// <returns></returns>
        [HttpGet("assessments/{assessmentId}/history")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        public IActionResult EventHistoryGet(int assessmentId)
        {
            return Ok(this._db.EventDetailHistory.Include(x => x.User).Where(x => x.AssessmentId == assessmentId).ToList());
        }

        [HttpPost("assessments/{assessmentId}/events/{eventId}/delete")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EventsDelete(int assessmentId, int eventId)
        {
            var o = await this._db.Events.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == eventId);
            this._db.EventDetails.RemoveRange(o.Details);
            await this._db.SaveChangesAsync();
            this._db.Events.Remove(o);
            await this._db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("assessments/{assessmentId}/events/{eventId}/details/{eventDetailId}/delete")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EventDetailsDelete(int assessmentId, int eventId, int eventDetailId)
        {
            var o = await this._db.EventDetails.FirstOrDefaultAsync(x => x.Id == eventDetailId);
            this._db.EventDetails.Remove(o);
            await this._db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("assessments/{assessmentId}/events/{eventId}/associate/sct/{sctId}")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EventsAssociateSct(int assessmentId, int eventId, int sctId)
        {
            var o = await this._db.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (string.IsNullOrEmpty(o.AssociatedSCTs))
            {
                o.AssociatedSCTs = sctId.ToString();
            }
            else
            {
                var a = o.AssociatedSCTs.Split(",").ToList();
                a.Remove(sctId.ToString());
                a.Add(sctId.ToString());
                o.AssociatedSCTs = string.Join(",", a);
            }

            var user = await this._db.Users.FirstOrDefaultAsync(x => x.Id == this.UserId);
            var sct = await this._db.METScts.FirstOrDefaultAsync(x => x.Id == sctId);

            var historyItem = new EventDetailHistory
            {
                AssessmentId = assessmentId,
                Created = DateTime.UtcNow,
                Message = $"SCT associated: {sct.Name}",
                EventId = eventId,
                User = user,
                HistoryType = "SYS"
            };

            this._db.EventDetailHistory.Update(historyItem);
            this._db.Events.Update(o);
            await this._db.SaveChangesAsync();

            //int eventId, string userId, string historyType, string message, string created)
            await this._hubContext.Clients.All.SendAsync("note",
                historyItem.EventId,
                user.FirstName,
                historyItem.HistoryType,
                historyItem.Message,
                historyItem.Created.ToString(CultureInfo.InvariantCulture)
            );

            return Ok();
        }

        [HttpPost("assessments/{assessmentId}/events/{eventId}/details/{eventDetailId}/associate/sct/{sctId}")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EventDetailsAssociateSct(int assessmentId, int eventId, int eventDetailId, int sctId)
        {
            var o = await this._db.EventDetails.FirstOrDefaultAsync(x => x.Id == eventDetailId);
            if (string.IsNullOrEmpty(o.AssociatedSCTs))
            {
                o.AssociatedSCTs = sctId.ToString();
            }
            else
            {
                var a = o.AssociatedSCTs.Split(",").ToList();
                a.Remove(sctId.ToString());
                a.Add(sctId.ToString());
                o.AssociatedSCTs = string.Join(",", a);
            }

            var user = await this._db.Users.FirstOrDefaultAsync(x => x.Id == this.UserId);
            var sct = await this._db.METScts.FirstOrDefaultAsync(x => x.Id == sctId);

            var historyItem = new EventDetailHistory
            {
                AssessmentId = assessmentId,
                Created = DateTime.UtcNow,
                Message = $"SCT associated: {sct.Name}",
                EventId = eventId,
                User = user,
                HistoryType = "SYS"
            };

            this._db.EventDetailHistory.Update(historyItem);
            this._db.EventDetails.Update(o);
            await this._db.SaveChangesAsync();

            //int eventId, string userId, string historyType, string message, string created)
            await this._hubContext.Clients.All.SendAsync("note",
                historyItem.EventId,
                user.FirstName,
                historyItem.HistoryType,
                historyItem.Message,
                historyItem.Created.ToString(CultureInfo.InvariantCulture)
            );

            return Ok();
        }

        [HttpPost("assessments/{assessmentId}/events/{eventId}/details/{eventDetailId}/associate/question/{questionId}")]
        [ProducesResponseType(typeof(Task<IActionResult>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EventsAssociateQuestion(int assessmentId, int eventId, int eventDetailId, int questionId)
        {
            var o = await this._db.EventDetails.FirstOrDefaultAsync(x => x.Id == eventDetailId);
            if (string.IsNullOrEmpty(o.AssociatedQuizQuestions))
            {
                o.AssociatedQuizQuestions = questionId.ToString();
            }
            else
            {
                var a = o.AssociatedQuizQuestions.Split(",").ToList();
                a.Remove(questionId.ToString());
                a.Add(questionId.ToString());
                o.AssociatedQuizQuestions = string.Join(",", a);
            }

            this._db.EventDetails.Update(o);
            await this._db.SaveChangesAsync();
            return Ok();
        }
    }
}