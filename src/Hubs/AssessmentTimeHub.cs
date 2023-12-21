// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Services;

namespace Seer.Hubs
{
    public class AssessmentTimeHub : HubBase
    {
        private static readonly ConnectionMapping<string> _connections = new();

        public AssessmentTimeHub(ApplicationDbContext context) : base(context)
        {
        }

        public void Join()
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);
            _connections.Add(assessmentId.ToString(), Context.ConnectionId);
        }

        public async Task Time(string status, int elapsed)
        {
            var assessmentId = Convert.ToInt32(this.GetHttpContext().Request.Query["assessmentid"]);

            var assessmentTimeService = new AssessmentTimeService(this._context);
            await assessmentTimeService.Get(assessmentId);
            var t = assessmentTimeService.Time;

            foreach (var connectionId in _connections.GetConnections(assessmentId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("time", t.Status, t.ElapsedTime);
            }
        }
    }
}