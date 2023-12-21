// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;

namespace Seer.Infrastructure.Models
{
    public class TeamScoringRequest
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class TeamScore
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public Score Participants { get; set; } = new();
        public Score CasesClosed { get; set; } = new();
        public Score TasksClosed { get; set; } = new();
        public Score CustomFieldsAnswered { get; set; } = new();
        public Score Observables { get; set; } = new();
        public Score Attachments { get; set; } = new();
        public Score CaseLogs { get; set; } = new();
        public Score Entries { get; set; } = new();
        public Score MinutesElapsed { get; set; } = new();
        public Score SummaryScore { get; set; } = new();

        public IEnumerable<UserScore> UserScores { get; set; }
        public IEnumerable<EventScore> EventScores { get; set; }

        public class UserScore
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public DateTime Created { get; set; }
            public Score Activities { get; set; } = new();

            public UserScore() { }

            public UserScore(User user)
            {
                this.Id = user.Id;
                this.FirstName = user.FirstName;
                this.LastName = user.LastName;
                this.Created = user.Created;
                this.Email = user.Email;
                this.UserName = user.UserName;
            }

            public UserScore(UserScore user)
            {
                this.Id = user.Id;
                this.FirstName = user.FirstName;
                this.LastName = user.LastName;
                this.Created = user.Created;
                this.Email = user.Email;
                this.UserName = user.UserName;
            }
        }

        public class EventScore
        {
            public int EventId { get; set; }
            public string EventName { get; set; }
            public Score Participants { get; set; } = new();
            public Score MinutesElapsed { get; set; } = new();
            public Score TasksClosed { get; set; } = new();
            public Score CustomFieldsAnswered { get; set; } = new();
            public Score Observables { get; set; } = new();
            public Score Attachments { get; set; } = new();
            public Score CaseLogs { get; set; } = new();
            public Score Entries { get; set; } = new();
            public IEnumerable<UserScore> UserScores { get; set; }
            public Score SummaryScore { get; set; } = new();
        }

        public class Score
        {
            public int Value { get; set; }
            public decimal Percentage { get; set; }
        }
    }
}