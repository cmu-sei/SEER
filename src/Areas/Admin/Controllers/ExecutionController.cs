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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/[controller]")]
    public class ExecutionController : BaseController
    {
        public ExecutionController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet("summary")]
        public async Task<ActionResult> Summary()
        {
            var ct = new CancellationToken();
            var list = await _db.Events.Include(o => o.Details).Include(o => o.AssignedTo)
                .OrderBy(o => o.DisplayOrder).Where(x => x.AssessmentId == this.AssessmentId)
                .ToListAsync(ct);
            return View(list);
        }

        [HttpGet("deployment")]
        public async Task<ActionResult> Deployment()
        {
            var ct = new CancellationToken();
            var list = await _db.Events.Include(o => o.Details).Include(o => o.AssignedTo)
                .OrderBy(o => o.DisplayOrder).Where(x => x.AssessmentId == this.AssessmentId)
                .ToListAsync(ct);
            return View(list);
        }

        [HttpGet("exsum")]
        public ActionResult Exsum()
        {
            return View();
        }
    }
}