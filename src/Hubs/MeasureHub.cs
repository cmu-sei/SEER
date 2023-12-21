// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;

namespace Seer.Hubs
{
    public class MeasureHub : HubBase
    {
        private static readonly ConnectionMapping<string> _connections = new();
        private IHubContext<ExecutionHub> _executionHubContext;

        public MeasureHub(ApplicationDbContext context, IHubContext<ExecutionHub> executionHubContext) : base(context)
        {
            this._executionHubContext = executionHubContext;
        }

        public async Task Measure(int eventId, int sctId, string comment, int status)
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"].ToString());
            var userId = this.UserId;
            
            var sct = await this._context.METScts.FirstOrDefaultAsync(x => x.Id == sctId);
            
            var s = await this._context.METItemSCTScores.FirstOrDefaultAsync(x => x.SCTId == sctId && x.AssessmentEventId == eventId);
            if (s == null)
            {
                s = new METItemSCTScore
                {
                    Comments = comment,
                    METId = sct.MetItemId,
                    SCTId = sctId,
                    AssessmentEventId = eventId,
                    SCTScore = (METItemSCTScore.Score)status,
                    UserId = userId
                };
                this._context.METItemSCTScores.Add(s);
            }
            else
            {
                s.Comments = comment;
                s.SCTScore = (METItemSCTScore.Score)status;
                this._context.METItemSCTScores.Update(s);
            }

            var user = await this._context.Users.FirstOrDefaultAsync(x => x.Id == this.UserId);
            var assessmentEvent = await this._context.Events.FirstOrDefaultAsync(x => x.Id == eventId);

            if (assessmentEvent != null)
            {
                var sctIds = assessmentEvent.AssociatedSCTs.ToIntList().ToList();
                if (!sctIds.Contains(sctId))
                {
                    sctIds.Add(sctId);
                }
                assessmentEvent.AssociatedSCTs = string.Join(",", sctIds);
                this._context.Update(assessmentEvent);
                
                var historyItem = new EventDetailHistory
                {
                    AssessmentId = assessmentId,
                    Created = DateTime.UtcNow,
                    Message = $"SCT updated: {s.SCTScore} {comment}",
                    EventId = assessmentEvent.Id,
                    User = user,
                    HistoryType = "EO"
                };

                this._context.EventDetailHistory.Update(historyItem);

                //int eventId, string userId, string historyType, string message, string created)
                await this._executionHubContext.Clients.All.SendAsync("note",
                    historyItem.EventId,
                    user.FirstName,
                    historyItem.HistoryType,
                    historyItem.Message,
                    historyItem.Created.ToString(CultureInfo.InvariantCulture)
                );
            }

            await this._context.SaveChangesAsync();

            foreach (var connectionId in _connections.GetConnections(assessmentId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("measure", eventId, sctId, comment, status);
            }
        }

        public override Task OnConnectedAsync()
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"].ToString());
            
            _connections.Add(assessmentId.ToString(), this.Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"].ToString());
            
            _connections.Remove(assessmentId.ToString(), this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
