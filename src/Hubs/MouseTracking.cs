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
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Seer.Infrastructure.Data;

namespace Seer.Hubs
{
    public class MouseTrackingHub : HubBase
    {
        private static readonly ConnectionMapping<string> _connections = new();

        public MouseTrackingHub(ApplicationDbContext context) : base(context) { }

        public async Task Move(int x, int y, string u)
        {
            var quizId = Convert.ToInt32(this.GetHttpContext().Request.Query["quizid"].ToString());

            foreach (var connectionId in _connections.GetConnections(quizId.ToString()))
            {
                await Clients.Client(connectionId).SendAsync("Move", this.UserName, x, y, u);
            }
        }

        public override Task OnConnectedAsync()
        {
            var name = this.UserName;
            var quizId = Convert.ToInt32(this.GetHttpContext().Request.Query["quizid"].ToString());

            _connections.Add(quizId.ToString(), Context.ConnectionId);
            _log.Debug($"Hub connection: Quiz: {quizId} Name:{name} ConnId:{Context.ConnectionId}");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var name = this.UserName;
            var quizId = Convert.ToInt32(this.GetHttpContext().Request.Query["quizid"].ToString());

            _connections.Remove(name, Context.ConnectionId);
            _log.Debug($"Hub disconnect: Quiz: {quizId} Name:{name} ConnId:{Context.ConnectionId}");

            return base.OnDisconnectedAsync(exception);
        }
    }
}