// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Services;

namespace Seer.Hubs
{
    public class ExecutionHub : HubBase
    {
        private static readonly ConnectionMapping<string> _connections = new();
        private ILogger<ExecutionHub> _logger;

        public ExecutionHub(ILogger<ExecutionHub> logger, ApplicationDbContext context) : base(context)
        {
            this._logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);
            _connections.Add(assessmentId.ToString(), Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);
            _connections.Remove(assessmentId.ToString(), this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task TimelineEvent(int eventId, int displayOrder, string assignedTo, string name, string extendedName, string timeScheduled)
        {
            _logger.LogDebug("Processing timeline event update {EventId}, {AssignedTo}", eventId, assignedTo);

            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);

            var u = await this._context.Users.FirstOrDefaultAsync(x => x.Id == assignedTo);
            var assessmentEvent = await this._context.Events.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(eventId));
            assessmentEvent.Name = name;
            assessmentEvent.AssignedTo = u;
            assessmentEvent.DisplayOrder = Convert.ToInt32(displayOrder);
            assessmentEvent.ExtendedName = extendedName;
            assessmentEvent.TimeScheduled = TimeSpan.Parse(timeScheduled);

            this._context.Events.Update(assessmentEvent);
            await this._context.SaveChangesAsync();

            foreach (var connectionId in _connections.GetConnections(assessmentId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("timelineevent",
                    eventId, displayOrder, assignedTo, name, extendedName,
                    timeScheduled);
            }
        }

        public async Task Refresh()
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);
            foreach (var connectionId in _connections.GetConnections(assessmentId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("refresh");
            }
        }

        public async Task MarkTime(int eventId, string type, string complete, string time)
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);
            var assessmentEvent = await this._context.Events.FirstOrDefaultAsync(x => x.Id == eventId);

            var assessmentTimeService = new AssessmentTimeService(this._context);
            await assessmentTimeService.Get(assessmentId);

            if (assessmentTimeService.Time.Status != AssessmentTime.ExerciseTimeStatus.Active)
            {
                assessmentTimeService.Time.Status = AssessmentTime.ExerciseTimeStatus.Active;
                await assessmentTimeService.Set();
            }

            var timeReturned = string.Empty;
            var isComplete = complete.Equals("true", StringComparison.InvariantCultureIgnoreCase);
            if (type.Equals("start", StringComparison.InvariantCultureIgnoreCase))
            {
                if (isComplete)
                {
                    assessmentEvent.TimeStart = TimeSpan.FromSeconds(assessmentTimeService.Time.ElapsedTime);
                    timeReturned = $"+{assessmentEvent.TimeStart.Value.ToString(@"hh\:mm", null)}";
                }
                else
                {
                    assessmentEvent.TimeStart = null;
                }
            }
            else
            {
                if (isComplete)
                {
                    assessmentEvent.TimeEnd = TimeSpan.FromSeconds(assessmentTimeService.Time.ElapsedTime);
                    timeReturned = $"+{assessmentEvent.TimeEnd.Value.ToString(@"hh\:mm", null)}";
                }
                else
                {
                    assessmentEvent.TimeEnd = null;
                }
            }

            this._context.Events.Update(assessmentEvent);
            await this._context.SaveChangesAsync();

            foreach (var connectionId in _connections.GetConnections(assessmentId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("marktime", eventId, type, complete, timeReturned);
            }
        }

        public async Task Note(int eventId, string userId, string historyType, string message, string created)
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);

            var user = await this._context.Users.FirstOrDefaultAsync(x => x.Id == this.UserId);

            var o = new EventDetailHistory
            {
                AssessmentId = assessmentId,
                Created = DateTime.UtcNow,
                Message = message,
                EventId = eventId,
                User = user,
                HistoryType = historyType
            };

            this._context.EventDetailHistory.Update(o);
            await this._context.SaveChangesAsync();

            foreach (var connectionId in _connections.GetConnections(assessmentId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("note", o.EventId, user.Name(),
                    "NOTE", o.Message, o.Created.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}