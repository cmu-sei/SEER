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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Seer.Hubs;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class QuestionsController : BaseController
    {
        private IHubContext<QuizHub> _hubContext;
        public QuestionsController(ApplicationDbContext dbContext, IDataProtectionProvider protector,
            IHubContext<QuizHub> hubcontext) : base(dbContext, protector)
        {
            this._hubContext = hubcontext;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> View(int id)
        {
            ViewBag.Quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == id);

            var questions = await _db.Questions
                .Where(q => q.QuizId == id)
                .OrderBy(q => q.Index)
                .ToArrayAsync();

            return View(questions);
        }

        [HttpGet("create")]
        public async Task<ActionResult> Create(int id)
        {
            ViewBag.Quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == id);

            return View(new QuizQuestion(id));
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create(int id, QuizQuestion model)
        {
            if (IsValidDataURL(model) == false)
            {
                throw new ApplicationException("Invalid Data URL.");
            }

            if (ModelState.IsValid == false)
            {
                return View(model);
            }
            model.Id = 0;
            model.OwnerUserId = this.UserId;
            model.Created = DateTime.UtcNow;
            model.QuizId = id;
            await _db.Questions.AddAsync(model);
            await _db.SaveChangesAsync();

            return RedirectToAction("View", new { id });
        }

        [HttpGet("{id}/edit")]
        public async Task<ActionResult> Edit(int id)
        {
            var question = await _db.Questions.FindAsync(id);
            ViewBag.Quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == question.QuizId);

            return View(question);
        }

        [HttpPost("{id}/edit")]
        public async Task<ActionResult> Edit(int id, QuizQuestion model)
        {
            var question = await _db.Questions.FindAsync(id);

            if (ModelState.IsValid == false)
            {
                return View(model);
            }

            question.Body = model.Body;
            question.Comment = model.Comment;
            question.Hint = model.Hint;
            question.Option1 = model.Option1;
            question.Option2 = model.Option2;
            question.Option3 = model.Option3;
            question.Option4 = model.Option4;
            question.Option5 = model.Option5;
            question.Option6 = model.Option6;
            question.Type = model.Type;
            question.AnsweredBy = model.AnsweredBy;
            question.AnsweredIndex = model.AnsweredIndex;
            question.AnsweredText = model.AnsweredText;
            question.CorrectIndex = model.CorrectIndex;
            question.CorrectText = model.CorrectText;
            question.Index = model.Index;

            _db.Update(question);
            await _db.SaveChangesAsync();

            return RedirectToAction("View", new { id = model.QuizId });
        }

        [HttpPost("{id}/delete")]
        public async Task<ActionResult> DeleteEx(int id, [FromQuery(Name = "g")] string goToUri)
        {
            var question = await _db.Questions.FindAsync(id);

            _db.Questions.Remove(question);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return RedirectToAction("View", new { id = question.QuizId });
        }

        [HttpPost("{id}/correct")]
        public async Task<ActionResult> Correct(int id)
        {
            var question = await this._db.Questions.FindAsync(id);

            var answer = new QuizAnswer
            {
                QuestionId = Convert.ToInt32(id),
                UserId = this.UserId,
                AnsweredIndex = Convert.ToInt32(question.CorrectIndex),
                AnsweredText = question.CorrectText,
                Status = QuizAnswer.AnswerStateType.Pending,
                Created = DateTime.UtcNow
            };
            await this._db.Answers.AddAsync(answer);
            await this._db.SaveChangesAsync();

            await this._hubContext.Clients.All.SendAsync("AnswerChanged", this.UserId, "Quiz Administrator", question.QuizId, question.Id, question.CorrectIndex, question.CorrectText);

            return RedirectToAction("Print", "Reports");
        }

        private bool IsValidDataURL(QuizQuestion model)
        {
            return model.GetOptions(false)
                .Select(opt => opt.OptionImage ?? "")
                .All(url => Regex.IsMatch(url, @"(^data:image/\w+;\w+,[0-9a-zA-Z/+=]+$)|(^$)"));
        }
    }
}