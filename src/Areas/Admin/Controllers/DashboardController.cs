// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Threading.Tasks;
using Seer.Areas.Admin.ViewModels;
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
    public class DashboardController : BaseController
    {
        public DashboardController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector) { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var c = new DashboardViewModel
            {
                Campaigns = await this._db.Campaigns
                    .Include(x => x.Operations)
                    .ThenInclude(x => x.Assessments)
                    .ToArrayAsync(),
                Groups = await this._db.Groups.ToArrayAsync()
            };
            return View(c);
        }
    }
}