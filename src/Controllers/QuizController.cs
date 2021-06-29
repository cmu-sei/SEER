/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Linq;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class QuizController : BaseController
    {
        public QuizController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index(int id)
        {
            var model = new QuizViewModel();

            var quiz = _db.Quizzes.FirstOrDefault(a => a.Id == id);

            if (quiz == null)
                return View();

            if (quiz.Status != Quiz.QuizStatus.InProgress)
                return RedirectToAction("Inactive", "Quiz");

            quiz.Questions = _db.Questions.Where(a => a.QuizId == id).OrderBy(o => o.Index).ToList();

            model.Created = quiz.Created;
            model.CreatedBy = quiz.CreatedBy;
            model.Id = quiz.Id;
            model.Name = quiz.Name;
            model.Questions = quiz.Questions;
            model.Status = quiz.Status;

            foreach (var question in model.Questions)
            {
                var a = _db.Answers.Where(o => o.QuestionId == question.Id).OrderByDescending(o => o.Created).Take(1).FirstOrDefault();
                if (a == null) continue;

                question.HintTaken = !string.IsNullOrEmpty(a.HintTakenBy);

                if (a.AnsweredIndex < 1 && string.IsNullOrEmpty(a.AnsweredText)) continue;

                var user = _db.Users.FirstOrDefault(o => o.Id == a.UserId);

                if (user != null) question.AnsweredBy = user.FirstName;
                question.AnsweredIndex = a.AnsweredIndex;
                question.AnsweredText = a.AnsweredText;
            }

            return View(model);
        }

        [HttpGet("inactive")]
        public ActionResult Inactive()
        {
            return View();
        }
    }
}
