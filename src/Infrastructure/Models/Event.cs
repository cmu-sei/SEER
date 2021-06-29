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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seer.Infrastructure.Models
{
    [Table("assessment_events")]
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DisplayOrder { get; set; }
        public string Name { get; set; }
        public string ExtendedName { get; set; }
        public string ExecutionTime { get; set; }
        public string ExecutiveSummary { get; set; }
        public string StoryLine { get; set; }
        public string TargetedSystems { get; set; }
        public IList<EventDetail> Details { get; set; } = new List<EventDetail>();

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }

        [ForeignKey("CatalogId")]
        public int CatalogId { get; set; }
        public TimeSpan TimeScheduled { get; set; }
        public TimeSpan? TimeStart { get; set; }
        public TimeSpan? TimeEnd { get; set; }
        public User AssignedTo { get; set; }

        public int? ScoreDiscovery { get; set; }
        public int? ScoreRemoval { get; set; }
        public int? ScoreSeverity { get; set; }
        public int? ScoreIntelligence { get; set; }

        public string AssociatedSCTs { get; set; }

        [NotMapped] public IList<EventDetailHistory> History { get; set; } = new List<EventDetailHistory>();

        public Event() { }

        public Event(AssessmentEventCatalogItem o)
        {
            this.Name = o.Name;
            this.ExtendedName = o.ExtendedName;
            this.ExecutiveSummary = o.ExecutiveSummary;
            this.StoryLine = o.Storyline;
            this.TargetedSystems = o.TargetedSystems;
            this.CatalogId = o.Id;
            foreach (var detail in o.Details)
            {
                var d = new EventDetail { Name = detail.Name, Procedures = detail.Procedures, AttackProfile = detail.AttackProfile };
                this.Details.Add(d);
            }
        }
    }

    [Table("assessment_event_details")]
    public class EventDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DisplayOrder { get; set; }
        public string Name { get; set; }
        public string Procedures { get; set; }
        public string AttackProfile { get; set; }
        public string AssociatedSCTs { get; set; }
        public string AssociatedQuizQuestions { get; set; }

        [NotMapped] public IList<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
        [NotMapped] public IList<METItemSCT> SCTs { get; set; } = new List<METItemSCT>();

        [NotMapped] public IList<EventDetailHistory> History { get; set; } = new List<EventDetailHistory>();
    }

    [Table("assessment_event_history")]
    public class EventDetailHistory
    {
        private string _historyAction;
        private string _historyType;
        private string _historyObject;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public EventHistoryStatus Status { get; set; } = EventHistoryStatus.NotReviewedAccepted;

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }

        [ForeignKey("EventId")]
        public int EventId { get; set; }

        public int? EventDetailId { get; set; }

        public User User { get; set; }

        public string HistoryType
        {
            get => _historyType;
            set => _historyType = value.ToUpper().Trim();
        }

        public string HistoryObject
        {
            get => _historyObject;
            set => _historyObject = value.ToUpper().Trim();
        }

        public string HistoryAction
        {
            get => _historyAction;
            set => _historyAction = value.ToUpper().Replace("CREATION", "CREATE").Trim();
        }

        public string Message { get; set; }

        /// <summary>
        /// Other system Id
        /// </summary>
        public string IntegrationId { get; set; }

        public string IntegrationRequestId { get; set; }

        public string IntegrationObject { get; set; }

        public string Tags { get; set; }

        public IEnumerable<string> GetTags()
        {
            if (string.IsNullOrEmpty(this.Tags)) return new List<string>();
            return this.Tags.Split(",");
        }

        public void SetTags(string value)
        {
            if (value != null) this.Tags = string.Join(",", value);
        }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public enum EventHistoryStatus
        {
            /// <summary>
            /// Inactive, deleted, etc.
            /// </summary>
            Unclassified = 0,
            /// <summary>
            /// Default
            /// </summary>
            [Display(Name = "Not Reviewed, Accepted")]
            NotReviewedAccepted = 1,
            [Display(Name = "Reviewed, Accepted")]
            ReviewedAccepted = 2,
            [Display(Name = "Reviewed, Incomplete")]
            ReviewedIncomplete = -5,
            [Display(Name = "Reviewed, Incorrect")]
            ReviewedIncorrect = -9
        }
    }
}