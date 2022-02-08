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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Hubs
{
    public class MeasureHub : HubBase
    {
        private static readonly ConnectionMapping<string> _connections = new();
        private IHubContext<ExecutionHub> _hubContext;

        public MeasureHub(ApplicationDbContext context, IHubContext<ExecutionHub> hubcontext) : base(context)
        {
            this._hubContext = hubcontext;
        }

        public async Task Measure(int eventId, int sctId, string comment, int status)
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"].ToString());
            var userId = this.UserId;
            
            var sct = await this._context.METScts.FirstOrDefaultAsync(x => x.Id == sctId);
            
            var s = await this._context.METItemSCTScores.FirstOrDefaultAsync(x => x.SCTId == sctId);
            if (s == null)
            {
                s = new METItemSCTScore
                {
                    Comments = comment,
                    METId = sct.MetItemId,
                    SCTId = sctId,
                    SCTScore = (METItemSCTScore.Score)status,
                    UserId = userId
                };
                await this._context.METItemSCTScores.AddAsync(s);
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
                assessmentEvent.AssociatedSCTs = sctId.ToString();
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
                await this._hubContext.Clients.All.SendAsync("note",
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
