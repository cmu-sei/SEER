// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.DataProtection;
using Seer.Infrastructure.Data;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class SurveyController : BaseController
    {
        public int SurveyId { get; set; }
        
        public SurveyController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index(int? id)
        {
            if(!this.AssessmentId.HasValue)
                return RedirectToAction("Index", "Home");
            
            var model = id.HasValue ? _db.Surveys.FirstOrDefault(o => o.Id == id) : _db.Surveys.FirstOrDefault(o => o.AssessmentId == this.AssessmentId);

            if (model == null)
            {
                model = Create();
                if (model == null)
                    return RedirectToAction("Inactive", "Survey", new { n = "404" });
            }

            if (model.Status != Survey.SurveyStatus.InProgress)
                return RedirectToAction("Inactive", "Survey");

            model.Questions = _db.SurveyQuestions.Where(o => o.SurveyId == model.Id).OrderBy(o => o.Id).ToList();

            foreach (var question in model.Questions)
            {
                var a = _db.SurveyAnswers.Where(o => o.QuestionId == question.Id && o.UserId == this.UserId).OrderByDescending(o => o.Created).Take(1).FirstOrDefault();
                if (a == null) continue;

                if (a.AnsweredIndex < 1 && string.IsNullOrEmpty(a.AnsweredText)) continue;

                question.AnsweredIndex = a.AnsweredIndex;
                question.AnsweredText = a.AnsweredText;
            }

            //going to leave open for now
            //if (model.Status != Survey.SurveyStatus)
            //  return RedirectToAction("Complete", "Survey");

            return View(model);
        }

        [HttpPost]
        public ActionResult Save(Survey model)
        {
            var userId = this.UserId;
            var id = model.Id;
            model = _db.Surveys.FirstOrDefault(o => o.Id == id);

            if (model == null)
                return RedirectToAction("Inactive", "Survey", new { n = "404" });

            if (model.Status != Survey.SurveyStatus.InProgress)
                return RedirectToAction("Inactive", "Survey");

            foreach (var key in Request.Form.Keys)
            {
                if (!key.StartsWith("q-")) continue;
                
                var qid = Convert.ToInt32(key.Replace("q-", ""));
                var t = Request.Form[$"o-{qid}"];
                    
                var answer = new SurveyAnswer
                {
                    QuestionId = qid,
                    Created = DateTime.UtcNow,
                    UserId = userId,
                    AnsweredText = Request.Form[$"q-{qid}"]
                };

                if (t.ToString().ToLower().Equals("multiplechoice"))
                {
                    answer = new SurveyAnswer
                    {
                        QuestionId = qid,
                        Created = DateTime.UtcNow,
                        UserId = userId,
                        AnsweredIndex = Convert.ToInt32(Request.Form[$"q-{qid}"])
                    };
                }

                _db.SurveyAnswers.Add(answer);
                _db.SaveChanges();
            }

            _db.Entry(model).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("Complete", "Survey");
        }

        [HttpGet("inactive")]
        public ActionResult Inactive()
        {
            return View();
        }

        [HttpGet("complete")]
        public ActionResult Complete()
        {
            return View();
        }

        private Survey Create()
        {
            var model = new Survey
            {
                Status = Survey.SurveyStatus.InProgress,
                AssessmentId = this.AssessmentId.Value,
                Created = DateTime.UtcNow,
                Name = $"Exercise Survey for {this.GroupName} {this.AssessmentName}"
            };

            _db.Surveys.Add(model);
            _db.SaveChanges();

            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Option3 = "3",
                Option4 = "4",
                Option5 = "5",
                Comment = "<style>.options {display: inline; margin-right: 12px;}</style>",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.MultipleChoice,
                SurveyId = model.Id,
                Body = "1. REALISM<br/>a) On a scale of 1-5, 1 being entirely unrealistic and 5 being very realistic, please rate the realism of this exercise compared to your real-world operational mission?"
            });
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.Text,
                SurveyId = model.Id,
                Body = "b) How could we improve upon the realism of the exercise?"
            });
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Option3 = "3",
                Option4 = "4",
                Option5 = "5",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.MultipleChoice,
                SurveyId = model.Id,
                Body = "2. TEAM TRAINING VALUE<br/>a) On a scale of 1–5, 1 being no team training value and 5 being the best possible team training value, how much team training value did this exercise provide?"
            });
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.Text,
                SurveyId = model.Id,
                Body = "b) How could we improve the team training value?"
            });
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Option3 = "3",
                Option4 = "4",
                Option5 = "5",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.MultipleChoice,
                SurveyId = model.Id,
                Body =
                    "3. INDIVIDUAL TRAINING VALUE<br/>a) On a scale of 1–5, 1 being no individual training value and 5 being the best possible individual training value, how much individual training value did this exercise provide?"
            }); _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.Text,
                SurveyId = model.Id,
                Body = "b) How could we improve the individual training value?"
            });
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Option3 = "3",
                Option4 = "4",
                Option5 = "5",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.MultipleChoice,
                SurveyId = model.Id,
                Body =
                    "4. EXERCISE PLANNING AND DEVELOPMENT<br/>a) On a scale of 1 - 5, 1 being completely unprepared and 5 being fully prepared, how prepared were you for this exercise?"
            });
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.Text,
                SurveyId = model.Id,
                Body = "b) How could you have been better prepared for the exercise?"
            });
            _db.SurveyQuestions.Add(new SurveyQuestion
            {
                Option1 = "1",
                Option2 = "2",
                Created = DateTime.UtcNow,
                Type = SurveyQuestion.SurveyQuestionType.Text,
                SurveyId = model.Id,
                Body =
                    "5. ADDITIONAL FEEDBACK<br/>Please include any additional comments you may have that would help us to improve the quality and value of your training."
            });

            _db.SaveChanges();

            return model;
        }
    }
}
