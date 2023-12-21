// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Services
{
    public class AssessmentScoringService
    {
        private readonly ApplicationDbContext _dbContext;

        public AssessmentScoringService(ApplicationDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<IEnumerable<TeamScore>> ScoreTeams(IEnumerable<TeamScoringRequest> requests)
        {
            var scores = new List<TeamScore>();
            foreach (var request in requests)
            {
                scores.Add(await this.ScoreTeam(request));
            }

            return scores;
        }

        public IEnumerable<TeamScore> Compare(IEnumerable<TeamScore> scores)
        {
            var teamScores = scores.ToList().OrderBy(x => x.AssessmentName).ThenBy(x => x.GroupName);

            var events = new List<TeamScore.EventScore>();
            foreach (var teamScore in teamScores)
            {
                events.AddRange(teamScore.EventScores);
            }

            foreach (var teamScore in teamScores)
            {
                teamScore.Attachments.CalculatePercentOfMax(teamScores.Max(x => x.Attachments.Value));
                teamScore.Entries.CalculatePercentOfMax(teamScores.Max(x => x.Entries.Value));
                teamScore.Observables.CalculatePercentOfMax(teamScores.Max(x => x.Observables.Value));
                teamScore.Participants.CalculatePercentOfMax(teamScores.Max(x => x.Participants.Value));
                teamScore.CaseLogs.CalculatePercentOfMax(teamScores.Max(x => x.CaseLogs.Value));
                teamScore.CasesClosed.CalculatePercentOfMax(teamScores.Max(x => x.CasesClosed.Value));
                // teamScore.MinutesElapsed.CalculatePercentOfMax(teamScores.Max(x => x.MinutesElapsed.Value));
                teamScore.TasksClosed.CalculatePercentOfMax(teamScores.Max(x => x.TasksClosed.Value));
                teamScore.CustomFieldsAnswered.CalculatePercentOfMax(teamScores.Max(x => x.CustomFieldsAnswered.Value));

                teamScore.SummaryScore.Value = teamScore.Attachments.Value +
                                               teamScore.Entries.Value +
                                               teamScore.Observables.Value +
                                               teamScore.Participants.Value +
                                               teamScore.CaseLogs.Value +
                                               teamScore.CasesClosed.Value +
                                               // we do not include time as score-able
                                               //teamScore.MinutesElapsed.Value +
                                               teamScore.TasksClosed.Value +
                                               teamScore.CustomFieldsAnswered.Value;

                teamScore.SummaryScore.Percentage = (teamScore.Attachments.Percentage +
                     teamScore.Entries.Percentage +
                     teamScore.Observables.Percentage +
                     teamScore.Participants.Percentage +
                     teamScore.CaseLogs.Percentage +
                     teamScore.CasesClosed.Percentage +
                     // teamScore.MinutesElapsed.Percentage +
                     teamScore.TasksClosed.Percentage +
                     teamScore.CustomFieldsAnswered.Percentage) / 9;

                foreach (var eventScore in teamScore.EventScores)
                {
                    eventScore.Attachments.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.Attachments.Value));
                    eventScore.Entries.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.Entries.Value));
                    eventScore.Observables.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.Observables.Value));
                    eventScore.Participants.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.Participants.Value));
                    eventScore.CaseLogs.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.CaseLogs.Value));
                    eventScore.MinutesElapsed.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.MinutesElapsed.Value));
                    eventScore.TasksClosed.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.TasksClosed.Value));
                    eventScore.CustomFieldsAnswered.CalculatePercentOfMax(events.Where(x => x.EventName == eventScore.EventName).Max(x => x.CustomFieldsAnswered.Value));

                    eventScore.SummaryScore.Value = eventScore.Attachments.Value +
                                                    eventScore.Entries.Value +
                                                    eventScore.Observables.Value +
                                                    eventScore.Participants.Value +
                                                    eventScore.CaseLogs.Value +
                                                    // eventScore.MinutesElapsed.Value +
                                                    eventScore.TasksClosed.Value +
                                                    eventScore.CustomFieldsAnswered.Value;

                    eventScore.SummaryScore.Percentage = (eventScore.Attachments.Percentage +
                          eventScore.Entries.Percentage +
                          eventScore.Observables.Percentage +
                          eventScore.Participants.Percentage +
                          eventScore.CaseLogs.Percentage +
                          // eventScore.MinutesElapsed.Percentage +
                          eventScore.TasksClosed.Percentage +
                          eventScore.CustomFieldsAnswered.Percentage) / 8;
                }
            }

            return teamScores;
        }

        public async Task<TeamScore> ScoreTeam(TeamScoringRequest request)
        {
            var score = new TeamScore
            {
                AssessmentName = request.AssessmentName,
                AssessmentId = request.AssessmentId,
                GroupId = request.GroupId,
                GroupName = request.GroupName
            };
            var eventScores = new List<TeamScore.EventScore>();

            var assessmentEvents = await this._dbContext.Events
                .Where(x => x.AssessmentId == request.AssessmentId)
                .ToListAsync();

            var assessmentHistories = await this._dbContext.EventDetailHistory
                .Include(x => x.User)
                .Where(x => x.AssessmentId == request.AssessmentId &&
                    // only score accepted
                    (x.Status == EventDetailHistory.EventHistoryStatus.ReviewedAccepted || x.Status == EventDetailHistory.EventHistoryStatus.NotReviewedAccepted)
                )
                .ToListAsync();

            // startex and endex
            var assessmentTimeService = new AssessmentTimeService(this._dbContext);
            await assessmentTimeService.Get(request.AssessmentId);

            score.UserScores = assessmentHistories.Select(x => x.User).Distinct()
                .Where(x => !x.Email.ToLower().Contains("hive_admin") &&
                    !x.Email.ToLower().Contains("step-admin") &&
                    !x.Email.ToLower().Equals("cpt"))
                .Select(user => new TeamScore.UserScore(user)).ToList();

            // Participants
            score.Participants.Value = score.UserScores.Count();

            // Cases Closed
            score.CasesClosed.Value += assessmentHistories
                .Count(x => x.Message.ToLower().Contains("completed") && x.Message.ToLower().Contains("case"));

            foreach (var assessmentEvent in assessmentEvents)
            {
                if (assessmentEvent.CatalogId < 1) continue; //ignore things like startex and endex events

                var eventScore = new TeamScore.EventScore { EventId = assessmentEvent.Id, EventName = assessmentEvent.Name };

                var eventUserScores = new List<TeamScore.UserScore>();
                foreach (var user in score.UserScores)
                {
                    var activities = assessmentHistories.Count(x => x.User.Id == user.Id && x.EventId == assessmentEvent.Id);
                    if (activities < 1) continue;
                    var eventUserScore = new TeamScore.UserScore(user) { Activities = { Value = activities } };
                    eventUserScores.Add(eventUserScore);

                    user.Activities.Value += eventUserScore.Activities.Value;
                }

                eventScore.UserScores = eventUserScores;
                eventScore.Participants.Value = eventScore.UserScores.Select(x => x.Id).Distinct().Count();

                // Tasks Closed
                eventScore.TasksClosed.Value = assessmentHistories
                    .Where(x => x.EventId == assessmentEvent.Id)
                    .Count(x => x.Message != null && x.Message.ToLower().Contains("completed") && x.Message.ToLower().Contains("task"));

                // Custom Fields Answered
                eventScore.CustomFieldsAnswered.Value = assessmentHistories
                    .Where(x => x.EventId == assessmentEvent.Id)
                    .Count(x => x.Message != null && x.Message.ToLower().Contains("updated customfields"));

                // Observables
                eventScore.Observables.Value = assessmentHistories
                    .Where(x => x.EventId == assessmentEvent.Id)
                    .Count(x => x.HistoryObject != null && x.HistoryObject.ToLower().Contains("case_artifact"));

                // Attachments
                eventScore.Attachments.Value = assessmentHistories
                    .Where(x => x.EventId == assessmentEvent.Id)
                    .Where(x => x.IntegrationObject != null && x.IntegrationObject.ToLower().Contains("attachment"))
                    .Select(x => x.IntegrationId)
                    .Distinct()
                    .Count();

                // Case Logs
                eventScore.CaseLogs.Value = assessmentHistories
                    .Where(x => x.EventId == assessmentEvent.Id)
                    .Count(x => x.HistoryObject != null && x.HistoryObject.ToLower().Contains("case_task_log"));

                // Entries
                eventScore.Entries.Value = assessmentHistories
                    .Count(x => x.EventId == assessmentEvent.Id);

                // minutes elapsed
                var tRecord = assessmentHistories
                    .OrderBy(x => x.Created)
                    .FirstOrDefault(x =>
                        x.EventId == assessmentEvent.Id &&
                        !x.User.Email.ToLower().Contains("step-admin") &&
                        !x.User.Email.ToLower().Contains("hive")) ?? new EventDetailHistory { Created = DateTime.UtcNow };
                var start = tRecord.Created;
                tRecord = assessmentHistories
                    .OrderByDescending(x => x.Created)
                    .FirstOrDefault(x =>
                        x.EventId == assessmentEvent.Id &&
                        !x.User.Email.ToLower().Contains("step-admin") &&
                        !x.User.Email.ToLower().Contains("hive")) ?? new EventDetailHistory { Created = DateTime.UtcNow };
                var end = tRecord.Created;

                if (assessmentHistories.Count < 1)
                {
                    end = assessmentTimeService.Time.EndTime ?? DateTime.UtcNow;
                }

                eventScore.MinutesElapsed.Value = Convert.ToInt32((end - start).TotalMinutes);

                eventScores.Add(eventScore);

                score.TasksClosed.Value += eventScore.TasksClosed.Value;
                score.CustomFieldsAnswered.Value += eventScore.CustomFieldsAnswered.Value;
                score.Observables.Value += eventScore.Observables.Value;
                score.Attachments.Value += eventScore.Attachments.Value;
                score.CaseLogs.Value += eventScore.CaseLogs.Value;
                score.Entries.Value += eventScore.Entries.Value;
                score.MinutesElapsed.Value += eventScore.MinutesElapsed.Value;
            }

            score.EventScores = eventScores;
            return score;
        }
    }
}