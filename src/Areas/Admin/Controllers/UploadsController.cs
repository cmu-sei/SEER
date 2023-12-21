// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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
