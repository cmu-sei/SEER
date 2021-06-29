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
using System.Linq;
using Seer.Hubs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Seer.Infrastructure.Data;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class QuizzesController : BaseController
    {
        private readonly IHubContext<QuizHub> _hubContext;

        public QuizzesController(ApplicationDbContext dbContext, IDataProtectionProvider protector,
            IHubContext<QuizHub> hubcontext) : base(dbContext, protector)
        {
            this._hubContext = hubcontext;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var assessment = await _db.Assessments.Include(x => x.Quizzes).FirstOrDefaultAsync(o => o.Id == this.AssessmentId);
            if (assessment == null) return NotFound();

            assessment.Quizzes = assessment.Quizzes.OrderBy(o => o.Index).ToList();
            foreach (var quiz in assessment.Quizzes)
            {
                var u = _db.Users.FirstOrDefault(o => o.Id == quiz.CreatedBy);
                if (u != null)
                    quiz.CreatedBy = u.Name();

            }
            return View(assessment.Quizzes);
        }

        [HttpGet("create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Quiz quiz)
        {
            if (!ModelState.IsValid) return View(quiz);

            quiz.Created = DateTime.Now;
            quiz.CreatedBy = this.UserId;
            quiz.AssessmentId = this.AssessmentId.Value;

            await _db.Quizzes.AddAsync(quiz);
            await _db.SaveChangesAsync();

            _log.Debug($"Quiz created: {quiz.Name} by {User.Identity?.Name}");

            return RedirectToAction("Index");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Edit(int id)
        {
            var quiz = await _db.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Quiz quiz)
        {
            quiz.Created = DateTime.Now;
            quiz.CreatedBy = this.UserId;
            quiz.AssessmentId = this.AssessmentId.Value;

            if (!ModelState.IsValid) return View(quiz);

            _db.Entry(quiz).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            _log.Debug($"Quiz edited: {quiz.Name} by {User.Identity?.Name}");

            //update active quiz status
            if (quiz.Status != Quiz.QuizStatus.InProgress)
            {
                await this._hubContext.Clients.All.SendAsync("CurrentStateChanged", quiz.Id, quiz.Status.ToString());
            }

            return RedirectToAction("Index");
        }

        [HttpGet("copy")]
        public async Task<ActionResult> Copy([FromQuery(Name = "from")] int from, [FromQuery(Name = "to")] int to)
        {
            var assessmentFrom = await _db.Assessments.Include(x => x.Quizzes).ThenInclude(x => x.Questions).FirstOrDefaultAsync(o => o.Id == from);
            var assessmentTo = await _db.Assessments.Include(x => x.Quizzes).FirstOrDefaultAsync(o => o.Id == to);
            var userid = this.UserId;

            if (assessmentFrom == null || assessmentTo == null)
                return NotFound();

            if (assessmentTo.Quizzes.Any())
                return Json("Assessment already has Quizzes");

            foreach (var quiz in assessmentFrom.Quizzes)
            {
                var newQuiz = new Quiz
                {
                    Created = DateTime.UtcNow,
                    CreatedBy = userid,
                    AssessmentId = to,
                    Name = quiz.Name,
                    Status = Quiz.QuizStatus.New,
                    Index = quiz.Index
                };
                await _db.Quizzes.AddAsync(newQuiz);
                await _db.SaveChangesAsync();

                foreach (var question in quiz.Questions)
                {
                    await _db.Questions.AddAsync(new QuizQuestion
                    {
                        Index = question.Index,
                        Body = question.Body,
                        BodyFormat = question.BodyFormat,
                        Comment = question.Comment,
                        CommentFormat = question.CommentFormat,
                        CorrectIndex = question.CorrectIndex,
                        CorrectText = question.CorrectText,
                        Created = DateTime.UtcNow,
                        Hint = question.Hint,
                        HintTaken = false,
                        Option1 = question.Option1,
                        Option2 = question.Option2,
                        Option3 = question.Option3,
                        Option4 = question.Option4,
                        Option5 = question.Option5,
                        Option6 = question.Option6,
                        QuizId = newQuiz.Id,
                        OwnerUserId = userid,
                        Type = question.Type
                    });

                    await _db.SaveChangesAsync();
                }
            }

            return Json("OK");
        }

        [HttpGet("{id}/markcorrect")]
        public async Task<ActionResult> MarkCorrect(int id)
        {
            var answer = await _db.Answers.FirstOrDefaultAsync(o => o.Id == id);

            if (answer == null)
                return NotFound();

            var question = await _db.Questions.FirstOrDefaultAsync(o => o.Id == answer.QuestionId);

            if (question == null)
                return NotFound();

            answer.Status = QuizAnswer.AnswerStateType.Correct;
            _db.Entry(answer).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            question.CorrectIndex = answer.AnsweredIndex;
            question.CorrectText = answer.AnsweredText;

            _db.Entry(question).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Json("OK");
        }

        [HttpGet("{id}/markna")]
        public async Task<ActionResult> MarkNa(int id)
        {
            var answer = await _db.Answers.FirstOrDefaultAsync(o => o.Id == id);
            if (answer == null)
                return NotFound();

            answer.Status = QuizAnswer.AnswerStateType.NotApplicable;
            _db.Entry(answer).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Json("OK");
        }
    }
}
