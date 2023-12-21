// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.ViewModels;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class EventHistoryController : BaseController
    {
        public EventHistoryController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index(int assessmentId)
        {

            var items = await _db.EventDetailHistory
                .Where(x => x.AssessmentId == assessmentId
                            && x.HistoryAction != "CREATE"
                            && !x.Message.ToLower().Contains("status to completed")
                            && !x.Message.ToLower().Contains("owner to")
                            && !x.Message.ToLower().Contains("update user")
                            && !x.Message.ToLower().Contains("update alert")
                            && !x.Message.ToLower().Contains("severity to")
                            && !x.Message.ToLower().Contains("case delete")
                            && !x.Message.ToLower().Contains("status to cancel")
                            )
                .Select(x => new EventHistoryTableItem
                {
                    Id = x.Id,
                    Created = x.Created,
                    Message = x.Message,
                    AssessmentId = x.AssessmentId,
                    EventId = x.EventId,
                    Status = x.Status
                }).OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(items);
        }
    }
}