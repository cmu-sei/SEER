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