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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Seer.Infrastructure.Models
{
    [Table("quiz_answers")]
    public class QuizAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("QuestionId")]
        public int QuestionId { get; set; }
        public int AnsweredIndex { get; set; }
        public string AnsweredText { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AnswerStateType Status { get; set; }
        public string HintTakenBy { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AnswerStateType
        {
            [System.ComponentModel.Description("As Entered")]
            Entered = 1,
            Pending = 2,
            Correct = 3,
            Incorrect = 4,
            /// <summary>
            /// Used for questions that can't be answered in game, like "oh we blew up the router, making our ability to answer a question about x impossible"
            /// </summary>
            [System.ComponentModel.Description("Not Applicable")]
            NotApplicable = 5
        }
    }
}