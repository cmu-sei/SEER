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
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.XWPF.UserModel;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Extensions;
using Seer.Infrastructure.Models;

namespace Seer.Areas.EO.Controllers;

public class DocumentItem
{
    public int Index { get; set; }
    public string Character { get; set; }
    public string Flag { get; set; }
    public string Value { get; set; }

    public DocumentItem(int index, string character, string flag)
    {
        this.Index = index;
        this.Character = character;
        this.Flag = flag;
    }
    
    public override string ToString()
    {
        return "{{" + $"s{this.Index}{this.Character.Trim()}_{this.Flag}" + "}}";
    }
}

[Area("EO")]
[Authorize(Roles = "EO,Admin")]
[Route("eo/[controller]/")]
public class DocsController :  BaseController
{
    public DocsController(ApplicationDbContext dbContext, IDataProtectionProvider protector) : base(dbContext, protector)
    {
    }

    [HttpGet]
    public ActionResult Index()
    {
        var templates = new []
        {
            "11-RCC-0001 Conduct Department of Defense Information Network (DODIN) Operations.docx", 
            "13-RCC-9040 Conduct Defensive Cyberspace Operations (DCO).docx",
            "13-RCC-9045 Conduct Regional Cyber Center (RCC) Theater Operations.docx"
        };

        var writtenFiles = new List<string>();

        foreach (var t in templates)
        {
            var template = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "templates", t);
            var output = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "output", $"{this.GroupName}_{this.AssessmentName}_{t}");
           
            //build all possible keys: GO, NO/GO, N/A for each line... 
            var updateValues = new List<DocumentItem>();
            foreach (var i in Enumerable.Range(1, 6))
            {
                updateValues.Add(new DocumentItem(i, "", "g"));
                updateValues.Add(new DocumentItem(i, "", "n"));
                updateValues.Add(new DocumentItem(i, "", "a"));
                updateValues.AddRange(from c in "abcdefghijklmnopqrstuvwxyz" from f in new[] { "g", "n", "a" } select new DocumentItem(i, c.ToString(), f));
            }

            //now populate those key values
            //TODO
            var events = _db.Events.Where(x => x.AssessmentId == this.AssessmentId).OrderBy(x => x.DisplayOrder).ToList();
            var mets = _db.METs.Include(x=>x.METItems).ThenInclude(x=>x.METSCTs).Where(x => x.AssessmentId == this.AssessmentId).ToList();

            var scores = new List<METItemSCTScore>();
            foreach (var evt in events.Where(evt => !string.IsNullOrEmpty(evt.AssociatedSCTs)))
            {
                var sctIds = evt.AssociatedSCTs.ToIntList();
                foreach(var score in _db.METItemSCTScores.OrderByDescending(x=>x.Created).Where(x => sctIds.Contains(x.SCTId)))
                {
                    scores.Add(score);
                }
            }

            foreach (var met in mets)
            {
                IEnumerable<METItem> metItems = met.METItems;
                foreach (var metItem in metItems.Where(x => x.Name.StartsWith(t.Substring(0, t.IndexOf(" ", StringComparison.Ordinal))))
                             .OrderBy(x => x.Index))
                {
                    foreach (var sct in metItem.METSCTs.OrderBy(x => x.Index))
                    {
                        var updateValueKey = sct.Name.Substring(0, sct.Name.IndexOf(".", StringComparison.Ordinal));

                        var sctScores = scores.Where(score =>
                            score.SCTId == sct.Id);

                        if (sctScores.Any(x => x.SCTScore == METItemSCTScore.Score.Go))
                        {
                            var documentItem = updateValues.Find(x => x.Index + x.Character == updateValueKey && x.Flag == "g");
                            if (documentItem != null)
                            {
                                documentItem.Value = "X";
                            }
                        }
                        else if (sctScores.Any(x => x.SCTScore == METItemSCTScore.Score.NoGo))
                        {
                            var documentItem = updateValues.Find(x => x.Index + x.Character == updateValueKey && x.Flag == "n");
                            if (documentItem != null)
                            {
                                documentItem.Value = "X";
                            }
                        }
                        else if (sctScores.Any(x => x.SCTScore == METItemSCTScore.Score.NA))
                        {
                            var documentItem = updateValues.Find(x => x.Index + x.Character == updateValueKey && x.Flag == "a");
                            if (documentItem != null)
                            {
                                documentItem.Value = "X";
                            }
                        }
                    }
                }
            }

            using var rs = System.IO.File.OpenRead(template);
            using var ws = System.IO.File.Create(output);
            var doc = new XWPFDocument(rs);

            foreach (var table in doc.Tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        foreach (var para in cell.Paragraphs)
                        {
                            foreach (var updateValue in updateValues)
                            {
                                var x = updateValue.ToString();
                                if (para.Text.Contains(x))
                                {
                                    para.ReplaceText(x, updateValue.Value);
                                }
                            }
                        }
                    }
                }
            }
            
            doc.Write(ws);
            writtenFiles.Add(output);
        }

        var zipName = $"{this.GroupName}_{this.AssessmentName}.zip";
        var zipPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "output", zipName);
        if(System.IO.File.Exists(zipPath))
            System.IO.File.Delete(zipPath);
        
        using(var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (var writtenFile in writtenFiles)
            {
                zip.CreateEntryFromFile(writtenFile, Path.GetFileName(writtenFile));
            }
        }


        var finalResult = System.IO.File.ReadAllBytes(zipPath);
        if (System.IO.File.Exists(zipPath)) { 
            System.IO.File.Delete(zipPath);
        }
        
        return File(finalResult, "application/zip", zipName);
    }
}