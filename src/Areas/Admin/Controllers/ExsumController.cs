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
using System.Text;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Seer.Hubs;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Services;
using Seer.Infrastructure.ViewModels;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class ExsumController : BaseController
    {
        public ExsumController(ApplicationDbContext dbContext, IHubContext<ExecutionHub> executionHubContext, IDataProtectionProvider protector) : base(dbContext, protector)
        {
            this._db = dbContext;
            this._executionHubContext = executionHubContext;
        }

        [HttpGet("coverpage")]
        public ActionResult CoverPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            if (this.AssessmentId < 1 || this.GroupId < 1 || string.IsNullOrEmpty(this.AssessmentName) || string.IsNullOrEmpty(this.GroupName))
                return Redirect("/admin/dashboard?e=no_id");

            ViewBag.AssessmentId = this.AssessmentId;

            var c = await this._db.Campaigns
                .Include(x => x.Operations).ThenInclude(x => x.Assessments).ThenInclude(x => x.Events).ThenInclude(x => x.Details)
                .Include(x => x.Operations).ThenInclude(x => x.Assessments).ThenInclude(x => x.Quizzes).ThenInclude(x => x.Questions)
                .Include(x => x.Operations).ThenInclude(x => x.Assessments).ThenInclude(x => x.METs).ThenInclude(x => x.METItems)
                .ThenInclude(x => x.METSCTs).ThenInclude(x => x.Score)
                .Include(x => x.DataPoints)
                .FirstOrDefaultAsync(o => o.Id == 1);

            if (c != null)
            {
                foreach (var element in c.Operations)
                {
                    foreach (var a in element.Assessments)
                    {
                        if (a.Id != this.AssessmentId) continue;

                        var assessmentTimeService = new AssessmentTimeService(this._db);
                        await assessmentTimeService.Get(this.AssessmentId.Value);
                        var time = assessmentTimeService.Time;

                        if (!time.StartTime.HasValue) continue;

                        a.ExecutionTime = time.StartTime.Value;
                        foreach (var timeHistory in time.History.Where(timeHistory =>
                                     timeHistory.Type == AssessmentTimesHistory.AssessmentTimesHistoryType.Start))
                        {
                            a.ExecutionTime = timeHistory.Created;
                        }

                        foreach (var assessmentEvent in a.Events)
                        {
                            assessmentEvent.History = await this._db.EventDetailHistory.Include(x => x.User)
                                .Where(x => x.EventId == assessmentEvent.Id && x.Status == EventDetailHistory.EventHistoryStatus.NotReviewedAccepted)
                                .OrderBy(x => x.Created)
                                .ToListAsync();

                            foreach (var d in assessmentEvent.Details)
                            {
                                if (!string.IsNullOrEmpty(d.AssociatedSCTs))
                                {
                                    foreach (var sct in d.AssociatedSCTs.Split(Convert.ToChar(",")))
                                    {
                                        var s = await _db.METScts.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(sct));
                                        if (s == null) continue;
                                        foreach (var met in a.METs)
                                        {
                                            foreach (var metItems in met.METItems)
                                            {
                                                foreach (var metItemSct in metItems.METSCTs)
                                                {
                                                    if (metItemSct.Id == s.Id)
                                                    {
                                                        s.Score = metItemSct.Score;
                                                    }
                                                }
                                            }
                                        }

                                        d.SCTs.Add(s);
                                    }
                                }

                                if (!string.IsNullOrEmpty(d.AssociatedQuizQuestions))
                                {
                                    foreach (var quiz in d.AssociatedQuizQuestions.Split(Convert.ToChar(",")))
                                    {
                                        var s = _db.Questions.FirstOrDefault(x => x.Id == Convert.ToInt32(quiz));
                                        if (s == null) continue;
                                        foreach (var assessmentQuizzes in a.Quizzes)
                                        {
                                            foreach (var assessmentQuestion in assessmentQuizzes.Questions)
                                            {
                                                if (assessmentQuestion.Id == s.Id)
                                                {
                                                    s.AnsweredIndex = assessmentQuestion.AnsweredIndex;
                                                    s.AnsweredBy = assessmentQuestion.AnsweredBy;
                                                    s.AnsweredText = assessmentQuestion.AnsweredText;
                                                    s.AnswerStatus = assessmentQuestion.AnswerStatus;
                                                }
                                            }
                                        }

                                        d.QuizQuestions.Add(s);
                                    }
                                }
                            }
                        }

                        foreach (var quiz in a.Quizzes)
                        {
                            foreach (var question in quiz.Questions)
                            {
                                var answers = await _db.Answers.Where(o => o.QuestionId == question.Id).OrderByDescending(o => o.Created)
                                    .ToListAsync();
                                foreach (var answer in answers)
                                {
                                    var user = await _db.Users.FirstOrDefaultAsync(o => o.Id == answer.UserId);

                                    question.AnsweredBy = $"{user.FirstName} {user.LastName}";
                                    question.AnsweredIndex = answer.AnsweredIndex;
                                    question.AnsweredText = answer.AnsweredText;
                                    question.AnswerStatus = answer.Status;
                                    question.HintTaken = !(string.IsNullOrEmpty(answer.HintTakenBy));

                                    break;
                                }
                            }
                        }

                        foreach (var met in a.METs)
                        {
                            foreach (var item in met.METItems)
                            {
                                foreach (var sct in item.METSCTs)
                                {
                                    var score = await _db.METItemSCTScores.OrderByDescending(o => o.Created)
                                        .FirstOrDefaultAsync(o => o.SCTId == sct.Id);
                                    sct.Score = score;
                                }
                            }
                        }
                    }
                }
            }
            return View(c);
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var exerciseEvents = await this._db.Events
                .Where(x => x.AssessmentId == this.AssessmentId)
                .OrderBy(x => x.DisplayOrder).ToListAsync();
            foreach (var item in exerciseEvents)
            {
                item.History = await this._db.EventDetailHistory.Where(x => x.EventId == item.Id).ToListAsync();
            }

            return View("User", exerciseEvents);
        }

        [HttpGet("timeline")]
        public IActionResult Timeline()
        {
            return View();
        }

        [HttpGet("score")]
        public async Task<IActionResult> Score()
        {
            var s = new AssessmentScoringService(this._db);
            var score = await s.ScoreTeam(
                new TeamScoringRequest
                {
                    AssessmentId = this.AssessmentId.Value,
                    AssessmentName = this.AssessmentName,
                    GroupId = this.GroupId.Value,
                    GroupName = this.GroupName
                });
            return Json(score);
        }

        [HttpGet("choose-compare")]
        public async Task<IActionResult> ChooseCompare()
        {
            var assessments = await this._db.Assessments.ToListAsync();
            var groups = await this._db.Groups.Where(x => assessments.Select(o => o.GroupId).Contains(x.Id)).ToListAsync();
            var assessmentGroups = new List<AssessmentAndGroup>();
            foreach (var assessment in assessments)
            {
                assessmentGroups.Add(new AssessmentAndGroup
                {
                    AssessmentId = assessment.Id,
                    AssessmentName = assessment.Name,
                    GroupId = assessment.GroupId,
                    GroupName = groups.FirstOrDefault(x => x.Id == assessment.GroupId)?.Name
                });
            }

            return View("choose-compare", assessmentGroups);
        }

        [HttpGet("compare")]
        public async Task<IActionResult> Compare(string assessmentIds)
        {
            List<int> ids;
            try
            {
                ids = assessmentIds.Split(Convert.ToChar(",")).Select(o => Convert.ToInt32(o)).ToList();
            }
            catch (Exception e)
            {
                return Redirect("choose-compare?e=" + e.Message);
            }

            var scores = new List<TeamScore>();
            var s = new AssessmentScoringService(this._db);
            var groups = await this._db.Groups.ToListAsync();
            foreach (var assessment in await this._db.Assessments.Where(x => ids.Distinct().Contains(x.Id)).ToListAsync())
            {
                var score = await s.ScoreTeam(
                    new TeamScoringRequest
                    {
                        AssessmentId = assessment.Id,
                        AssessmentName = assessment.Name,
                        GroupId = assessment.GroupId,
                        GroupName = groups.FirstOrDefault(x => x.Id == assessment.GroupId)?.Name
                    });
                scores.Add(score);
            }

            var comparison = s.Compare(scores);
            return View(comparison);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("files/data1.csv")]
        public async Task<FileResult> Download(string view)
        {
            if (this.AssessmentId < 1 || this.GroupId < 1 || string.IsNullOrEmpty(this.AssessmentName) || string.IsNullOrEmpty(this.GroupName))
                return null;

            const string fileName = "data1.csv";
            var content = new StringBuilder("event,start,end,startLabel,endLabel,region,timeline").Append(Environment.NewLine);

            var assessmentTimeService = new AssessmentTimeService(this._db);
            await assessmentTimeService.Get(this.AssessmentId.Value);
            var time = assessmentTimeService.Time;

            var executionTime = time.StartTime;
            foreach (var timeHistory in time.History.Where(timeHistory => timeHistory.Type == AssessmentTimesHistory.AssessmentTimesHistoryType.Start)
                    )
            {
                executionTime = timeHistory.Created;
            }

            var events = await this._db.Events.Where(x => x.AssessmentId == this.AssessmentId).ToListAsync();
            foreach (var ev in events.OrderBy(x => x.DisplayOrder))
            {
                var lastHistoryTime = ev.TimeEnd;

                var histories = await this._db.EventDetailHistory
                    .Where(x => x.AssessmentId == this.AssessmentId).ToListAsync();
                foreach (var history in histories.Where(x => x.EventId == ev.Id).OrderBy(x => x.Created))
                {
                    try
                    {
                        var resource = new IntegrationMessageConverterService(history.IntegrationObject, _db, _executionHubContext);

                        var created = Convert.ToDouble(resource.HiveObject.StartDate).FromJavaTimeStampToDateTime();
                        var duration = created.ToLocalTime() - executionTime.Value.ToLocalTime();

                        if (duration.TotalSeconds < 0) continue;

                        if (duration > new TimeSpan(0, 0, 0, Convert.ToInt32(assessmentTimeService.Time.ElapsedTime)))
                        {
                            //duration = assessmentTimeService.Time.ElapsedSeconds - (event started seconds)
                            duration = new TimeSpan(0, 0, 0,
                                Convert.ToInt32(assessmentTimeService.Time.ElapsedTime - ev.TimeStart.Value.TotalSeconds));
                        }

                        if (new[] { "step-admin", "hive_admin" }.Contains(resource.HiveObject.BaseObject.UpdatedBy)) continue;

                        if (string.IsNullOrEmpty(view) || view != "summary")
                        {
                            content.Append(resource.HiveObject.BaseObject.UpdatedBy).Append(" ");

                            var title = history.Message.ToLower();
                            title += " - " + resource.Detail.Message;
                            title = title.Clean();

                            content.Append(title).Append(',').Append(Convert.ToInt32(duration.TotalMinutes)).Append(',')
                                .Append(Convert.ToInt32(duration.TotalMinutes + 2)).Append(",,,");
                            content.Append(ev.Name).Append(",Inject").Append(Environment.NewLine);
                        }

                        lastHistoryTime = duration;
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                    }
                }


                ev.TimeStart ??= new TimeSpan(0);
                ev.TimeEnd ??= new TimeSpan(14400);
                lastHistoryTime ??= ev.TimeEnd;
                if (string.IsNullOrEmpty(ev.ExtendedName)) ev.ExtendedName = "";

                if (ev.Name == "STARTEX" || ev.Name == "ENDEX")
                {
                    if (!string.IsNullOrEmpty(ev.Name.Clean()))
                    {
                        content.Append(ev.Name.Clean()).Append(',').Append(Convert.ToInt32(ev.TimeStart.Value.TotalMinutes)).Append(',')
                            .Append(Convert.ToInt32(ev.TimeStart.Value.TotalMinutes + 10)).Append(",,,Exercise Event,Admin")
                            .Append(Environment.NewLine);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ev.ExtendedName.Clean()))
                    {
                        content.Append(ev.ExtendedName.Clean()).Append(',').Append(Convert.ToInt32(ev.TimeStart.Value.TotalMinutes)).Append(',')
                            .Append(Convert.ToInt32(lastHistoryTime.Value.TotalMinutes)).Append(",,,").Append(ev.Name)
                            .Append(",Inject").Append(Environment.NewLine);
                    }
                }
            }

            var fileBytes = Encoding.ASCII.GetBytes(content.ToString());
            return File(fileBytes, "text/csv", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> AddEvent([FromForm] int assessmentid, int eventid, int index, [FromQuery(Name = "g")] string goToUri)
        {
            var o = await _db.CatalogEvents.Include(x => x.Details).FirstOrDefaultAsync(x => x.Id == eventid);

            var evt = new Event(o);
            evt.AssessmentId = this.AssessmentId.Value;
            evt.DisplayOrder = index;
            await _db.Events.AddAsync(evt);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return Ok();
        }
    }
}