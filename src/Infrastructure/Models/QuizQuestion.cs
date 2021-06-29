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
using System.Linq;
using Seer.Infrastructure.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Seer.Infrastructure.Models
{
    [Table("quiz_questions")]
    public class QuizQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizId")]
        public int QuizId { get; set; }

        /// <summary>
        /// 0 based
        /// </summary>
        [Display(Name = "Display Index")]
        public int Index { get; set; }

        public QuestionType Type { get; set; }

        public string OwnerUserId { get; set; }

        [Display(Name = "Question"), Required]
        public string Body { get; set; }

        [Display(Name = "Question Text Format"), Required]
        public TextFormatType BodyFormat { get; set; }

        [Display(Name = "Option 1"), Required]
        public string Option1 { get; set; }
        [Display(Name = "Option Image 1")]
        public string OptionImage1 { get; set; }

        [Display(Name = "Option 2"), Required]
        public string Option2 { get; set; }
        [Display(Name = "Option Image 2")]
        public string OptionImage2 { get; set; }

        [Display(Name = "Option 3")]
        public string Option3 { get; set; }
        [Display(Name = "Option Image 3")]
        public string OptionImage3 { get; set; }

        [Display(Name = "Option 4")]
        public string Option4 { get; set; }
        [Display(Name = "Option Image 4")]
        public string OptionImage4 { get; set; }

        [Display(Name = "Option 5")]
        public string Option5 { get; set; }
        [Display(Name = "Option Image 5")]
        public string OptionImage5 { get; set; }

        [Display(Name = "Option 6")]
        public string Option6 { get; set; }
        [Display(Name = "Option Image 6")]
        public string OptionImage6 { get; set; }

        public string Hint { get; set; }

        [NotMapped]
        public QuizAnswer.AnswerStateType AnswerStatus { get; set; }

        [NotMapped]
        public int AnswerId { get; set; }

        /// <summary>
        /// Not mapped
        /// </summary>
        [NotMapped]
        public bool HintTaken { get; set; }
        /// <summary>
        /// Not mapped
        /// </summary>
        [NotMapped]
        public int AnsweredIndex { get; set; }
        /// <summary>
        /// Not mapped
        /// </summary>
        [NotMapped]
        public string AnsweredText { get; set; }
        /// <summary>
        /// Not mapped
        /// </summary>
        [NotMapped]
        public string AnsweredBy { get; set; }
        /// <summary>
        /// Not mapped
        /// </summary>
        [NotMapped]
        public bool IsAnsweredCorrectly { get; set; }

        public IEnumerable<OptionViewModel> GetOptions(bool trim = true)
        {
            var filter = trim ?
                opt => string.IsNullOrEmpty(opt.Option) == false :
                (Func<OptionViewModel, bool>)(_ => true);

            return new[] {
                new OptionViewModel(1, Option1, OptionImage1),
                new OptionViewModel(2, Option2, OptionImage2),
                new OptionViewModel(3, Option3, OptionImage3),
                new OptionViewModel(4, Option4, OptionImage4),
                new OptionViewModel(5, Option5, OptionImage5),
                new OptionViewModel(6, Option6, OptionImage6)
            }.Where(filter).ToArray();
        }

        [Display(Name = "Correct Answer")]
        public int CorrectIndex { get; set; }

        [Display(Name = "Correct Answer")]
        public string CorrectText { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Comment Format"), Required]
        public TextFormatType CommentFormat { get; set; }

        //public string Category { get; set; }

        [Display(Name = "Created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public QuizQuestion()
        {
        }

        public QuizQuestion(int quizId)
        {
            this.QuizId = quizId;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum TextFormatType
        {
            PlainText,
            MarkDown
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum QuestionType
        {
            [System.ComponentModel.Description("Multiple Choice")]
            MultipleChoice,
            [System.ComponentModel.Description("Text")]
            Text
        }

        public AnswerDetail GetAnswerDetail()
        {
            var o = new AnswerDetail();
            if (this.Type == QuestionType.MultipleChoice)
            {
                o.CorrectAnswer = this.CorrectIndex switch
                {
                    6 => this.Option6,
                    5 => this.Option5,
                    4 => this.Option4,
                    3 => this.Option3,
                    2 => this.Option2,
                    _ => this.Option1
                };

                if (!string.IsNullOrEmpty(this.AnsweredBy))
                {
                    o.Answer = this.AnsweredIndex switch
                    {
                        6 => this.Option6,
                        5 => this.Option5,
                        4 => this.Option4,
                        3 => this.Option3,
                        2 => this.Option2,
                        _ => this.Option1
                    };
                }
            }
            else
            {
                o.Answer = this.AnsweredText;
                o.CorrectAnswer = this.CorrectText;
            }

            return o;
        }

        public class AnswerDetail
        {
            public string Answer { get; set; }
            public string CorrectAnswer { get; set; }
        }
    }
}