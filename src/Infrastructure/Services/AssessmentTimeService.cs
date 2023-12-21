// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seer.Hubs;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services
{
    public class AssessmentTimeService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<AssessmentTimeHub> _hubContext;

        public AssessmentTime Time { get; set; } = new();

        public AssessmentTimeService()
        {
        }

        public AssessmentTimeService(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        public AssessmentTimeService(ApplicationDbContext dbContext, IHubContext<AssessmentTimeHub> hubContext)
        {
            this._db = dbContext;
            this._hubContext = hubContext;
        }

        public async Task Get(int assessmentId)
        {
            var model = await this.GetAssessmentTimeByAssessmentId(assessmentId);

            if (model == null)
            {
                model = new AssessmentTime
                {
                    AssessmentId = assessmentId,
                    Status = AssessmentTime.ExerciseTimeStatus.NotStarted
                };

                _db.AssessmentTime.Add(model);
                await _db.SaveChangesAsync();
            }

            this.Time = model;

            foreach (var history in this.Time.History.OrderByDescending(x => x.Created))
            {
                switch (history.Type)
                {
                    // this section accounts for fixing incorrect times
                    case AssessmentTimesHistory.AssessmentTimesHistoryType.End:
                        {
                            this.Time.Status = AssessmentTime.ExerciseTimeStatus.Ended;
                            if (!this.Time.EndTime.HasValue)
                            {
                                var existingItem = await this.GetAssessmentTimeByAssessmentId(model.AssessmentId);
                                existingItem.EndTime = history.Created;
                                _db.AssessmentTime.Update(existingItem);
                                await _db.SaveChangesAsync();
                                this.Time.EndTime = history.Created;
                            }

                            break;
                        }
                    case AssessmentTimesHistory.AssessmentTimesHistoryType.Start:
                        {
                            if (this.Time.StartTime != history.Created)
                            {
                                var existingItem = await this.GetAssessmentTimeByAssessmentId(model.AssessmentId);
                                existingItem.StartTime = history.Created;
                                _db.AssessmentTime.Update(existingItem);
                                await _db.SaveChangesAsync();
                                this.Time.StartTime = history.Created;
                            }

                            break;
                        }
                }
            }

            this.SetElapsed();
        }

        private async Task<AssessmentTime> GetAssessmentTimeByAssessmentId(int assessmentId)
        {
            return await _db.AssessmentTime
                .Include(x => x.History)
                .FirstOrDefaultAsync(o => o.AssessmentId == assessmentId);
        }

        public async Task<AssessmentTime> Set()
        {
            var existingItem = await this.GetAssessmentTimeByAssessmentId(this.Time.AssessmentId);

            if (existingItem == null)
            {
                this._db.AssessmentTime.Add(new AssessmentTime
                {
                    AssessmentId = this.Time.AssessmentId,
                    StartTime = DateTime.UtcNow,
                    Status = AssessmentTime.ExerciseTimeStatus.NotStarted
                });
                await _db.SaveChangesAsync();
                existingItem = await GetAssessmentTimeByAssessmentId(this.Time.AssessmentId);
            }

            if (this.Time.Status != AssessmentTime.ExerciseTimeStatus.NotStarted)
            {
                var type = AssessmentTimesHistory.AssessmentTimesHistoryType.Start;

                switch (this.Time.Status)
                {
                    case AssessmentTime.ExerciseTimeStatus.Paused:
                        type = AssessmentTimesHistory.AssessmentTimesHistoryType.Pause;
                        break;
                    case AssessmentTime.ExerciseTimeStatus.Active:
                        if (existingItem != null && existingItem.Status == AssessmentTime.ExerciseTimeStatus.Paused)
                        {
                            type = AssessmentTimesHistory.AssessmentTimesHistoryType.Resume;
                        }
                        else
                        {
                            type = AssessmentTimesHistory.AssessmentTimesHistoryType.Start;
                        }

                        break;
                    case AssessmentTime.ExerciseTimeStatus.Ended:
                        type = AssessmentTimesHistory.AssessmentTimesHistoryType.End;
                        existingItem.EndTime = DateTime.UtcNow;
                        break;
                    case AssessmentTime.ExerciseTimeStatus.NotStarted:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Status not handled: {this.Time.Status}");
                }

                if (existingItem != null)
                {
                    existingItem.Status = this.Time.Status;
                    existingItem.StartTime = this.Time.StartTime;

                    var h = new AssessmentTimesHistory
                    {
                        AssessmentTimeId = existingItem.Id,
                        Created = DateTime.UtcNow,
                        Type = type
                    };

                    existingItem.History.Add(h);
                }
            }

            _db.AssessmentTime.Update(existingItem);
            await _db.SaveChangesAsync();

            await this.SendToHubAsync(existingItem);
            return existingItem;
        }

        private void GetElapsed(AssessmentTime model)
        {
            this.Time = model;
            this.SetElapsed();
        }

        private void SetElapsed()
        {
            double elapsed = 0;
            var startTime = this.Time.StartTime;
            var endTime = DateTime.UtcNow;

            if (this.Time.History.Any(o => o.Type == AssessmentTimesHistory.AssessmentTimesHistoryType.End))
            {
                endTime = this.Time.History.LastOrDefault(o => o.Type == AssessmentTimesHistory.AssessmentTimesHistoryType.End).Created;
            }

            //any pauses? because then it is a straight calculation
            if (this.Time.History.All(o => o.Type != AssessmentTimesHistory.AssessmentTimesHistoryType.Pause))
            {
                foreach (var item in this.Time.History
                    .Where(o => o.Type == AssessmentTimesHistory.AssessmentTimesHistoryType.Start)
                    .OrderByDescending(o => o.Created))
                {
                    this.Time.ElapsedTime = (endTime - item.Created).TotalSeconds;
                    return;
                }
            }

            var items = this.Time.History.OrderBy(o => o.Created);
            foreach (var item in items)
            {
                switch (item.Type)
                {
                    case AssessmentTimesHistory.AssessmentTimesHistoryType.Start:
                        startTime = item.Created;
                        break;
                    case AssessmentTimesHistory.AssessmentTimesHistoryType.Pause:
                        elapsed += (item.Created - startTime.Value).TotalSeconds;
                        startTime = item.Created;
                        break;
                    case AssessmentTimesHistory.AssessmentTimesHistoryType.Resume:
                        startTime = item.Created;
                        elapsed += (endTime - item.Created).TotalSeconds;
                        break;
                    case AssessmentTimesHistory.AssessmentTimesHistoryType.End:
                        endTime = item.Created;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"History type is not handled {item.Type}");
                }
            }

            this.Time.ElapsedTime = elapsed;
        }

        public async Task SendToHubAsync(AssessmentTime model)
        {
            if (this._hubContext != null)
                await this._hubContext.Clients.All.SendAsync("Time", model.Status, model.ElapsedTime);
        }
    }
}