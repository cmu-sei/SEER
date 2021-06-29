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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Seer.Infrastructure.Models
{
    /// <summary>
    /// Quiz Status is New (0), InProgress (1), Closed (2), Deleted (3)
    /// </summary>
    [Table("quizzes")]
    public class Quiz
    {
        /// <summary>
        /// Created with new quiz
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// 0 based
        /// </summary>
        [Display(Name = "Display Index")]
        public int Index { get; set; }

        /// <summary>
        /// 0. New
        /// 1. InProgress
        /// 2. Closed
        /// 3. Deleted 
        /// </summary>
        public QuizStatus Status { get; set; }

        /// <summary>
        /// Saved on create
        /// </summary>
        public DateTime Created { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// UserId for creator
        /// </summary>
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        public List<QuizQuestion> Questions { get; set; } = new();

        /// <summary>
        /// For display purposes only (yes I know should be in viewmodel)
        /// </summary>
        [NotMapped]
        public double Score { get; set; }
        [NotMapped]
        public int GroupId { get; set; }

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }

        /// <summary>
        /// Quiz status affects its display to players - they only see InProgress quizzes
        /// 0. New
        /// 1. InProgress
        /// 2. Closed
        /// 3. Deleted
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum QuizStatus
        {
            /// <summary>
            /// 0. New quizzes
            /// </summary>
            New,
            /// <summary>
            /// 1. InProgress quizzes are accessible by players
            /// </summary>
            [System.ComponentModel.Description("In Progress")]
            InProgress,
            /// <summary>
            /// 2. Closed
            /// </summary>
            Closed,
            /// <summary>
            /// 3. Deleted
            /// </summary>
            Deleted
        }
    }
}