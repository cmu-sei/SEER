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