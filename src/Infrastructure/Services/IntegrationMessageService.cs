// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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

            if (model == null)
            {
                log.Warn($"EventHistoryStatusUpdate {statusUpdate.Id} not found");
                return false;
            }
            
            model.Status = statusUpdate.Status;
            dbContext.Update(model);
            await dbContext.SaveChangesAsync(ct);
            return true;
        }

        public static async Task<int> Associate(ApplicationDbContext dbContext, EventHistoryAssociation association, CancellationToken ct)
        {
            var model = await dbContext.EventDetailHistory
                .FirstOrDefaultAsync(o => o.Id == association.Id, ct);

            if (model == null)
            {
                log.Warn($"EventHistoryAssociation {association.Id} not found");
                return -1;
            }
            
            model.AssessmentId = association.AssessmentId;
            model.EventId = association.EventId;
            if(association.EventDetailId > 0)
                model.EventDetailId = association.EventDetailId;

            try
            {
                dbContext.Update(model);
                await dbContext.SaveChangesAsync(ct);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

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
            IHubContext<ExecutionHub> executionHubContext, UserManager<User> userManager,
            object payload, string historyType = "HIVE")
        {
            log.Debug(payload);
            try
            {
                var o = new IntegrationMessageConverterService(payload.ToString(), dbContext);

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
                
                var userService = new UserService(dbContext, userManager);
                var user = await userService.GetUserAsync(o.Detail.User.UserName);
                o.Detail.User = user;
                o.Detail.HistoryType = historyType;

                dbContext.EventDetailHistory.Add(o.Detail);
                await dbContext.SaveChangesAsync();

                //int eventId, string userId, string historyType, string message, string created)
                await executionHubContext.Clients.All.SendAsync("note",
                    o.Detail.EventId,
                    user.UserName,
                    o.Detail.HistoryType,
                    o.Detail.Message,
                    o.Detail.Created.ToString(CultureInfo.InvariantCulture)
                );
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static async Task Process(ApplicationDbContext dbContext,
            IHubContext<ExecutionHub> executionHubContext, UserManager<User> userManager,
            IntegrationMessage payload, string historyType = "IRIS")
        {
            log.Debug(payload);
            try
            {
                if (payload.Detail.AssessmentId < 0 || payload.Detail.EventId < 0)
                {
                    var associatedRecords = dbContext.EventDetailHistory
                        .Where(x => x.IntegrationId == payload.Detail.IntegrationId && (x.AssessmentId < 0 || x.EventId < 0));
                    foreach (var associatedRecord in associatedRecords)
                    {
                        associatedRecord.AssessmentId = payload.Detail.AssessmentId;
                        associatedRecord.EventId = payload.Detail.EventId;
                    }
                    await dbContext.SaveChangesAsync();
                }

                //determine user
                
                var userService = new UserService(dbContext, userManager);
                var user = await userService.GetUserAsync(payload.Detail.User.UserName);
                payload.Detail.User = user;
                payload.Detail.HistoryType = historyType;

                dbContext.EventDetailHistory.Add(payload.Detail);
                await dbContext.SaveChangesAsync();

                //int eventId, string userId, string historyType, string message, string created)
                await executionHubContext.Clients.All.SendAsync("note",
                    payload.Detail.EventId,
                    user.UserName,
                    payload.Detail.HistoryType,
                    payload.Detail.Message,
                    payload.Detail.Created.ToString(CultureInfo.InvariantCulture)
                );
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static async Task<string> Fix(ApplicationDbContext dbContext, IHubContext<ExecutionHub> executionHubContext)
        {
            var response = new StringBuilder();
            foreach (var history in await dbContext.EventDetailHistory.ToListAsync())
            {
                try
                {
                    var x = new IntegrationMessageConverterService(history.IntegrationObject, dbContext);
                    history.Message = x.Detail.Message;

                    var o = JsonConvert.DeserializeObject<HiveObject>(history.IntegrationObject);
                    if (!string.IsNullOrEmpty(o!.BaseObject.UpdatedBy))
                    {
                        var u = dbContext.Users.FirstOrDefault(z =>
                            string.Equals(z.Email, o.BaseObject.UpdatedBy, StringComparison.InvariantCultureIgnoreCase));
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

        public static AssessmentEvent FindId(ApplicationDbContext dbContext, HiveObject hiveObject)
        {
            var assessmentEvent = new AssessmentEvent();
            var o = dbContext.EventDetailHistory
                .FirstOrDefault(x=>x.IntegrationId == hiveObject.Id);

            if (o != null)
            {
                assessmentEvent.AssessmentId = o.AssessmentId;
                assessmentEvent.EventId = o.EventId;
            }

            return assessmentEvent;
        }
    }
}