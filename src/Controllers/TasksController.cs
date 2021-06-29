/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Seer.Infrastructure.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class TasksController : BaseController
    {
        public TasksController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public ActionResult Index()
        {
            if (!this.AssessmentId.HasValue)
                return RedirectToAction("Assessment", "Home");

            var aid = this.AssessmentId.Value;
            var model = new TaskingViewModel();

            var tasks = _db.TaskingItems.Include(o => o.TaskingItemDocuments).Where(o => o.AssessmentId == aid && o.Status == ActiveStatus.Active).ToList();
            model.Items = tasks;

            foreach (var item in model.Items)
            {
                var r = _db.TaskingItemResults.Where(o => o.TaskingItemId == item.Id);
                foreach (var res in r)
                {
                    model.Results.Add(res);
                }
            }

            model.GroupId = this.GroupId.Value;
            return View(model);
        }
    }
}
