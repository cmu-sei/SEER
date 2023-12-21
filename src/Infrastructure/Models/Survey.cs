// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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
    [Table("surveys")]
    public class Survey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public SurveyStatus Status { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public IList<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SurveyStatus
        {
            New,
            [System.ComponentModel.Description("In Progress")]
            InProgress,
            Closed
        }
    }

    [Table("survey_questions")]
    public class SurveyQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SurveyId { get; set; }

        public SurveyQuestionType Type { get; set; }

        [Display(Name = "Question"), Required]
        public string Body { get; set; }

        [Display(Name = "Option 1"), Required]
        public string Option1 { get; set; }

        [Display(Name = "Option 2"), Required]
        public string Option2 { get; set; }

        [Display(Name = "Option 3")]
        public string Option3 { get; set; }

        [Display(Name = "Option 4")]
        public string Option4 { get; set; }

        [Display(Name = "Option 5")]
        public string Option5 { get; set; }

        [Display(Name = "Option 6")]
        public string Option6 { get; set; }

        [NotMapped]
        public int AnsweredIndex { get; set; }
        [NotMapped]
        public string AnsweredText { get; set; }

        public IEnumerable<SurveyOptionViewModel> GetOptions(bool trim = true)
        {
            var filter = trim ?
                opt => string.IsNullOrEmpty(opt.Option) == false :
                (Func<SurveyOptionViewModel, bool>)(_ => true);

            return new[] {
                new SurveyOptionViewModel(1, Option1),
                new SurveyOptionViewModel(2, Option2),
                new SurveyOptionViewModel(3, Option3),
                new SurveyOptionViewModel(4, Option4),
                new SurveyOptionViewModel(5, Option5),
                new SurveyOptionViewModel(6, Option6)
            }.Where(filter).ToArray();
        }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Created")]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [NotMapped] public IList<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();

        public SurveyQuestion()
        {
        }

        public SurveyQuestion(int id)
        {
            this.SurveyId = id;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SurveyTextFormatType
        {
            PlainText,
            MarkDown
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SurveyQuestionType
        {
            [System.ComponentModel.Description("Multiple Choice")]
            MultipleChoice = 0,
            [System.ComponentModel.Description("Text")]
            Text = 1
        }
    }

    [Table("survey_answers")]
    public class SurveyAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int QuestionId { get; set; }
        public int AnsweredIndex { get; set; }
        public string AnsweredText { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}