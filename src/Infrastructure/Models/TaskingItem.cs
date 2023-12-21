// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Seer.Infrastructure.Enums;

namespace Seer.Infrastructure.Models
{
    [Table("tasking_items")]
    public class TaskingItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Index { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
        public string DetectEndpoint { get; set; }
        public string AnticipatedTeamAction { get; set; }
        public string TaskInformation { get; set; }
        public ActiveStatus Status { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public IList<TaskingItemDocument> TaskingItemDocuments { get; set; } = new List<TaskingItemDocument>();
        public IList<TaskingItemResult> TaskingItemResults { get; set; } = new List<TaskingItemResult>();

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }
    }

    [Table("tasking_item_results")]
    public class TaskingItemResult
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TaskingItemType
        {
            UserEntered,
            SystemEntered
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TaskingItemId { get; set; }
        public TaskingItemType Type { get; set; }
        public string UserId { get; set; }
        public int PercentComplete { get; set; }
        public bool IsComplete { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }

    [Table("tasking_item_documents")]
    public class TaskingItemDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
    }
}