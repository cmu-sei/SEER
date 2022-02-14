/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH
Copyright 2021 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING,
BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT,
TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution. Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM21-0384
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;

namespace Seer.Areas.EO.Controllers;

[Area("EO")]
[Authorize(Roles = "EO,Admin")]
[Route("eo/[controller]/")]
public class MeasureController :  BaseController
{
    public MeasureController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector)
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

                await _db.METs.AddAsync(met);
            }

            await _db.SaveChangesAsync();

            return Redirect(Request.Path);
        }
        
        var events = await _db.Events.Where(x => x.AssessmentId == this.AssessmentId).OrderBy(x => x.DisplayOrder).ToListAsync();
        var mets = await _db.METs.Include(x=>x.METItems).ThenInclude(x=>x.METSCTs).Where(x => x.AssessmentId == this.AssessmentId).ToListAsync();
        ViewBag.METs = mets;

        var scores = new List<METItemSCTScore>();
        foreach (var evt in events.Where(evt => !string.IsNullOrEmpty(evt.AssociatedSCTs)))
        {
            var sctIds = evt.AssociatedSCTs.ToIntList();
            foreach(var score in _db.METItemSCTScores.OrderByDescending(x=>x.Created).Where(x => sctIds.Contains(x.SCTId)))
            {
                scores.Add(score);
            }
        }

        ViewBag.Scores = scores;

        return View(events);
    }
}