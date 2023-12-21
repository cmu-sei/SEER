// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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