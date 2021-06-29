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