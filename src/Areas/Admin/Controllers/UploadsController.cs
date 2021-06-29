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
    public class UploadsController : BaseController
    {
        public UploadsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var model = await _db.Uploads.Where(o => o.AssessmentId == this.AssessmentId).ToListAsync();

            foreach (var item in model)
            {
                item.User = _db.Users.FirstOrDefault(o => o.Id == item.UserId);
                item.Group = _db.Groups.FirstOrDefault(o => o.Id == item.AssessmentId);
            }
            return View(model);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Download(int id)
        {
            var upload = await this._db.Uploads.FirstOrDefaultAsync(o => o.Id == id);
            if (upload == null)
                return NotFound(); //doc truly not found

            throw new NotImplementedException();

            //TODO
            //var filepath = Server.MapPath(upload.Location + upload.Id + Path.GetExtension(upload.Name));
            //var contentType = MimeMapping.GetMimeMapping(filepath);

            //return File(filepath, contentType, Server.UrlEncode(upload.Name));
        }
    }
}
