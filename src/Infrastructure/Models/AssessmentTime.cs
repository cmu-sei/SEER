// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Seer.Infrastructure.Models
{
    [Table("assessment_times")]
    public class AssessmentTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ExerciseTimeStatus Status { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [ForeignKey("AssessmentId")] public int AssessmentId { get; set; }

        [NotMapped] public double ElapsedTime { get; set; }

        public IList<AssessmentTimesHistory> History { get; } = new List<AssessmentTimesHistory>();

        public enum ExerciseTimeStatus
        {
            [Description("Not Started")] NotStarted = 0,
            Paused = 1,
            Active = 2,
            Ended = 3
        }
    }

    [Table("assessment_times_history")]
    public class AssessmentTimesHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AssessmentTimesHistoryType Type { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public int AssessmentTimeId { get; set; }

        public enum AssessmentTimesHistoryType
        {
            Start = 0,
            Pause = 1,
            Resume = 2,
            Correction = 3,
            End = 4
        }
    }
}