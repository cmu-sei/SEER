// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;
using Seer.Infrastructure.Services;

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
        return await LoadMetData(MetType.MET);
    }
    
    [HttpGet("jmets")]
    public async Task<ActionResult> Jmets()
    {
        return await LoadMetData(MetType.JMET);
    }

    private async Task<ActionResult> LoadMetData(MetType type)
    {
        var m = await _db.METs
            .Include(x => x.METItems)
            .ThenInclude(x => x.METSCTs)
            .FirstOrDefaultAsync(x => x.AssessmentId == this.AssessmentId.Value && x.Type == type);

        if (m == null)
        {
            //build new METL
            var assessmentId = this.AssessmentId;
            if (assessmentId != null)
            {
                var s = new MetsService(this._db); 
                await s.CreateMetl(assessmentId);
            }
        }
        
        var events = await _db.Events.Where(x => x.AssessmentId == this.AssessmentId).OrderBy(x => x.DisplayOrder).ToListAsync();
        var mets = await _db.METs.Include(x=>x.METItems).ThenInclude(x=>x.METSCTs).Where(x => x.AssessmentId == this.AssessmentId && x.Type == type).ToListAsync();
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

        var hasFiles = false;
        
        try
        {
            hasFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "templates") ,"*.docx", SearchOption.AllDirectories).Any();
        }
        catch { 
            //dir doesn't exist
        }
        
        ViewBag.Docs = hasFiles;
        
        return View(events);
    }
}