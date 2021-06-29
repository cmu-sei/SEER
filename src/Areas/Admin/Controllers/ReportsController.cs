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
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class ReportsController : BaseController
    {
        public ReportsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> EvaluationResults()
        {
            if (string.IsNullOrEmpty(Request.Query["quizid"]) || string.IsNullOrEmpty(Request.Query["groupid"]))
                return NotFound();

            var quizId = Convert.ToInt32(Request.Query["quizid"]);

            ViewBag.QuizId = quizId;
            ViewBag.GroupId = this.GroupId;
            ViewBag.GroupName = this.GroupName;
            ViewBag.AssessmentName = this.AssessmentName;

            var model = new List<Quiz>();

            var quiz = await _db.Quizzes.FirstOrDefaultAsync(o => o.Id == quizId);
            if (quiz == null)
                return NotFound();

            var correct = 0;
            quiz.Questions = await _db.Questions.Where(o => o.QuizId == quiz.Id).OrderBy(o => o.Index).ToListAsync();

            var validUsers = await _db.GroupUsers.Where(o => o.GroupId == this.GroupId).Select(o => o.UserId).ToListAsync();

            foreach (var question in quiz.Questions.OrderBy(o => o.Index))
            {
                var answer = await _db.Answers.Where(o => o.QuestionId == question.Id).OrderByDescending(o => o.Created).Take(1).FirstOrDefaultAsync();
                if (answer == null) continue;
                if (answer.AnsweredIndex < 1 && string.IsNullOrEmpty(answer.AnsweredText)) continue;

                if (!validUsers.Contains(answer.UserId))
                    continue;

                var user = await _db.Users.FirstOrDefaultAsync(o => o.Id == answer.UserId);

                question.AnswerStatus = answer.Status;

                question.AnswerId = answer.Id;
                question.AnsweredBy = user.FirstName;
                question.AnsweredIndex = answer.AnsweredIndex;
                question.AnsweredText = answer.AnsweredText;
                if (question.AnswerStatus == QuizAnswer.AnswerStateType.NotApplicable) continue;

                if (question.Type == QuizQuestion.QuestionType.MultipleChoice)
                    question.IsAnsweredCorrectly = question.CorrectIndex == answer.AnsweredIndex;
                else
                    question.IsAnsweredCorrectly = question.CorrectText == answer.AnsweredText;

                if (question.IsAnsweredCorrectly)
                    correct++;

            }
            var questionCount = quiz.Questions.Count(o => o.AnswerStatus != QuizAnswer.AnswerStateType.NotApplicable);
            quiz.Score = questionCount > 0 ? Convert.ToInt32(Convert.ToDouble(correct) / Convert.ToDouble(questionCount) * 100) : 0;
            model.Add(quiz);

            return View(model);
        }

        [HttpGet("print")]
        public async Task<ActionResult> Print()
        {
            var model = new List<Quiz>();

            var quizzes = await _db.Quizzes.Where(o => o.AssessmentId == this.AssessmentId).OrderBy(o => o.Index).ToListAsync();

            foreach (var quiz in quizzes)
            {
                var correct = 0;
                quiz.Questions = await _db.Questions.Where(o => o.QuizId == quiz.Id).OrderBy(o => o.Index).ToListAsync();

                var validUsers = await _db.GroupUsers.Where(o => o.GroupId == this.GroupId).Select(o => o.UserId).ToListAsync();

                foreach (var question in quiz.Questions.OrderBy(o => o.Index))
                {
                    var answer = await _db.Answers.Where(o => o.QuestionId == question.Id)
                        .OrderByDescending(o => o.Created).Take(1).FirstOrDefaultAsync();
                    if (answer == null) continue;
                    if (answer.AnsweredIndex < 1 && string.IsNullOrEmpty(answer.AnsweredText)) continue;

                    if (!validUsers.Contains(answer.UserId))
                        continue;

                    var user = await _db.Users.FirstOrDefaultAsync(o => o.Id == answer.UserId);

                    question.AnswerStatus = answer.Status;

                    question.AnsweredBy = user.FirstName;
                    question.AnsweredIndex = answer.AnsweredIndex;
                    question.AnsweredText = answer.AnsweredText;
                    if (question.AnswerStatus == QuizAnswer.AnswerStateType.NotApplicable) continue;

                    if (question.Type == QuizQuestion.QuestionType.MultipleChoice)
                        question.IsAnsweredCorrectly = question.CorrectIndex == answer.AnsweredIndex;
                    else
                        question.IsAnsweredCorrectly = question.CorrectText == answer.AnsweredText;

                    if (question.IsAnsweredCorrectly)
                        correct++;
                }
                var questionCount =
                    quiz.Questions.Count(o => o.AnswerStatus != QuizAnswer.AnswerStateType.NotApplicable);
                quiz.Score = questionCount > 0 ? Convert.ToInt32(Convert.ToDouble(correct) / Convert.ToDouble(questionCount) * 100) : 0;
                model.Add(quiz);
            }

            return View(model);
        }
    }
}