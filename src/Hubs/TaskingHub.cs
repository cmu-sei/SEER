// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.SignalR;
using Seer.Infrastructure.Data;

namespace Seer.Hubs
{
    public class TasksHub : HubBase
    {
        private static readonly ConnectionMapping<string> _connections = new();

        public TasksHub(ApplicationDbContext context) : base(context) { }

        public async Task AnswerChanged(int tid, string status, string comment)
        {
            var groupId = this.GetHttpContext().Request.Query["groupid"].ToString();
            var userId = Guid.Empty; //TODO

            var isComplete = !string.IsNullOrEmpty(status) && status.Equals("true", StringComparison.InvariantCultureIgnoreCase);

            this._context.TaskingItemResults.Add(new TaskingItemResult { Comment = comment, TaskingItemId = tid, UserId = userId.ToString(), IsComplete = isComplete });
            await this._context.SaveChangesAsync();

            foreach (var connectionId in _connections.GetConnections(groupId))
            {
                await Clients.Client(connectionId).SendAsync("answerchanged", tid, status, comment);
            }
        }

        public override Task OnConnectedAsync()
        {
            var groupId = this.GetHttpContext().Request.Query["groupid"].ToString();

            _connections.Add(groupId, this.Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var groupId = this.GetHttpContext().Request.Query["groupid"].ToString();

            _connections.Remove(groupId, this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
