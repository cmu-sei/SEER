// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Areas.EO.Controllers
{
    [Area("EO")]
    [Authorize(Roles = "EO,Admin")]
    [Route("eo/[controller]")]
    public class METLController : BaseController
    {
        public METLController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector)
        {
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var m = await _db.METs
                .Include(x => x.METItems)
                .ThenInclude(x => x.METSCTs)
                .FirstOrDefaultAsync(o => o.AssessmentId == this.AssessmentId.Value);

            if (m == null)
            {
                //build new METL
                var met = new MET { Name = "", Created = DateTime.UtcNow, AssessmentId = this.AssessmentId.Value };

                //load file from local config
                var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "config", "metl.txt");
                if (System.IO.File.Exists(path))
                {
                    var content = await System.IO.File.ReadAllTextAsync(path);
                    var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                    var metItem = new METItem();
                    var metIndex = 1;
                    var sctIndex = 1;

                    foreach (var line in lines)
                    {
                        var processedLine = line.Trim();

                        if (line.StartsWith("\t") || line.StartsWith(" "))
                        {
                            metItem.METSCTs.Add(new METItemSCT { Index = sctIndex, Name = processedLine, Status = ActiveStatus.Active, Title = "Step"});
                            sctIndex++;
                        }
                        else
                        {
                            metItem = new METItem { Index = metIndex, Name = processedLine };
                            metIndex++;
                            sctIndex = 1;
                        }

                        met.METItems.Add(metItem);
                    }

                    _db.METs.Add(met);
                }

                await _db.SaveChangesAsync();

                return Redirect(Request.Path);
            }

            var scores = await this._db.METItemSCTScores.Where(o => o.METId == m.Id).ToListAsync();

            foreach (var metitem in m.METItems)
            {
                foreach (var sct in metitem.METSCTs)
                {
                    foreach (var score in scores)
                    {
                        if (score.SCTId == sct.Id)
                        {
                            sct.Score = score;
                        }
                    }
                }
            }

            foreach (var metitem in m.METItems.ToList())
            {
                foreach (var sct in metitem.METSCTs.ToList())
                {
                    if (sct.SectionId > 0 && sct.SectionId != Convert.ToInt32(Request.Query["sectionid"]))
                    {
                        metitem.METSCTs.Remove(sct);
                    }
                }

                if (metitem.METSCTs.Count < 1)
                {
                    m.METItems.Remove(metitem);
                }
            }

            return View(m);
        }
    }
}