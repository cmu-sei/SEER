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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using Seer.Hubs;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.ViewModels;

namespace Seer.Infrastructure.Services
{
    public static class IntegrationMessageService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> UpdateStatus(ApplicationDbContext dbContext, EventHistoryStatusUpdate statusUpdate, CancellationToken ct)
        {
            var model = await dbContext.EventDetailHistory
                .FirstOrDefaultAsync(o => o.Id == statusUpdate.Id, ct);
            model.Status = statusUpdate.Status;
            dbContext.Update(model);
            await dbContext.SaveChangesAsync(ct);
            return true;
        }

        public static async Task<int> Associate(ApplicationDbContext dbContext, EventHistoryAssociation association, CancellationToken ct)
        {
            var model = await dbContext.EventDetailHistory
                .FirstOrDefaultAsync(o => o.Id == association.Id, ct);

            model.AssessmentId = association.AssessmentId;
            model.EventId = association.EventId;
            model.EventDetailId = association.EventDetailId;

            dbContext.Update(model);
            await dbContext.SaveChangesAsync(ct);

            return await TryMatchUnmatched(dbContext);
        }

        public static async Task<int> TryMatchUnmatched(ApplicationDbContext dbContext)
        {
            var updated = 0;

            var histories = await dbContext.EventDetailHistory.ToListAsync();

            foreach (var history in histories)
            {
                if (history.AssessmentId == -1 || history.EventId == -1)
                {
                    var referenceRecord = histories.FirstOrDefault(x =>
                        x.IntegrationId == history.IntegrationId && x.AssessmentId != -1 && x.EventId != -1);
                    if (referenceRecord != null)
                    {
                        history.AssessmentId = referenceRecord.AssessmentId;
                        history.EventId = referenceRecord.EventId;

                        dbContext.Entry(history).State = EntityState.Modified;
                    }
                }

                if (dbContext.Entry(history).State == EntityState.Modified)
                    updated++;
            }

            await dbContext.SaveChangesAsync();

            return updated;
        }

        public static async Task Process(ApplicationDbContext dbContext,
            IHubContext<ExecutionHub> hubContext,
            object payload)
        {
            log.Debug(payload);
            var o = new IntegrationMessageConverterService(payload.ToString());

            if (o.Detail.AssessmentId < 0 || o.Detail.EventId < 0)
            {
                var associatedRecords = dbContext.EventDetailHistory
                    .Where(x => x.IntegrationId == o.Detail.IntegrationId && (x.AssessmentId < 0 || x.EventId < 0));
                foreach (var associatedRecord in associatedRecords)
                {
                    associatedRecord.AssessmentId = o.Detail.AssessmentId;
                    associatedRecord.EventId = o.Detail.EventId;
                }
                await dbContext.SaveChangesAsync();
            }

            //determine user
            var user = await UserService.GetUserAsync(dbContext, o.Detail.User.UserName);
            o.Detail.User = user;
            o.Detail.HistoryType = "HIVE";

            await dbContext.EventDetailHistory.AddAsync(o.Detail);
            await dbContext.SaveChangesAsync();

            //int eventId, string userId, string historyType, string message, string created)
            await hubContext.Clients.All.SendAsync("note",
                o.Detail.EventId,
                user.UserName,
                o.Detail.HistoryType,
                o.Detail.Message,
                o.Detail.Created.ToString(CultureInfo.InvariantCulture)
            );
        }

        public static async Task<string> Fix(ApplicationDbContext dbContext)
        {
            var response = new StringBuilder();
            foreach (var history in await dbContext.EventDetailHistory.ToListAsync())
            {
                try
                {
                    var x = new IntegrationMessageConverterService(history.IntegrationObject);
                    history.Message = x.Detail.Message;

                    var o = JsonConvert.DeserializeObject<HiveObject>(history.IntegrationObject);
                    if (!string.IsNullOrEmpty(o.BaseObject.UpdatedBy))
                    {
                        var u = dbContext.Users.FirstOrDefault(x => x.Email.ToUpper() == o.BaseObject.UpdatedBy.ToUpper());
                        if (u != null)
                        {
                            history.User = u;
                        }
                    }

                    history.IntegrationRequestId = x.HiveObject.RequestId;

                    dbContext.Entry(history).State = EntityState.Modified;

                    response.AppendLine($"**{x.Detail.IntegrationId}: {x.Detail.Message}**");
                    foreach (var (key, value) in x.Updates)
                    {
                        response.AppendLine($"{key} : {value}");
                    }
                    response.AppendLine("");
                    response.AppendLine("");
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    response.AppendLine($"Error processing {history.Id}");
                }
            }

            await dbContext.SaveChangesAsync();

            return response.ToString();
        }
    }
}