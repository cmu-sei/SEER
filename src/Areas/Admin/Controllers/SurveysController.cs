// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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
    public class SurveysController : BaseController
    {
        public SurveysController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = await _db.Surveys
                .Include(x => x.Questions)
                .FirstOrDefaultAsync(o => o.AssessmentId == this.AssessmentId);

            if (model != null)
            {
                foreach (var question in model.Questions)
                {
                    var questionId = question.Id;
                    question.Answers = _db.SurveyAnswers.Where(o => o.QuestionId == questionId).ToList();
                }
            }
            else
            {
                model = new Survey();
            }
            return View(model);
        }
    }
}