// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Linq;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Enums;

namespace Seer.Controllers.API
{
    [Route("api/mets/")]
    public class METsController : BaseController
    {
        public METsController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpPost("{metId}/metitems/{metItemId}/delete")]
        public ActionResult DeleteMetItem(int metId, int metItemId, [FromQuery(Name = "g")] string goToUri)
        {
            _db.METScts.RemoveRange(_db.METScts.Where(x => x.MetItemId == metItemId));
            _db.SaveChanges();

            var o = _db.METItems.FirstOrDefault(x => x.Id == metItemId);
            _db.METItems.Remove(o);
            _db.SaveChanges();

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return Ok();
        }

        [HttpPost("{metId}/metitems/{metItemId}/scts/{sctId}/delete")]
        public ActionResult DeleteSctItem(int metId, int metItemId, int sctId, [FromQuery(Name = "g")] string goToUri)
        {
            var o = _db.METScts.FirstOrDefault(x => x.Id == sctId);
            _db.METScts.Remove(o);
            _db.SaveChanges();

            if (!string.IsNullOrEmpty(goToUri))
                return Redirect(goToUri);

            return Ok();
        }

        [HttpPost("{metId}/metitems")]
        public ActionResult SimpleCreate(int metId, SimpleUpdate update)
        {
            var model = new METItem();
            model.Created = DateTime.Now;
            model.Name = update.Name;
            model.MetId = metId;
            model.Index = update.Index;

            _db.METItems.Add(model);
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost("{metId}/metitems/{metItemId}/scts")]
        public ActionResult SimpleSctCreate(int metId, int metItemId, SimpleUpdate update)
        {
            var model = new METItemSCT
            {
                Created = DateTime.Now,
                Name = update.Name,
                MetItemId = metItemId,
                Index = update.Index,
                Status = ActiveStatus.Active
            };

            _db.METScts.Add(model);
            _db.SaveChanges();

            return Ok();
        }

        [HttpPost("{metId}/metitems/{metItemId}")]
        public async Task<ActionResult> SimpleEdit(int metId, int metItemId, SimpleUpdate update)
        {
            var o = _db.METItems.FirstOrDefault(x => x.Id == metItemId);
            if (o != null)
            {
                o.Name = update.Name;
                o.Index = update.Index;
                _db.METItems.Update(o);
            }

            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{metId}/metitems/{metItemId}/scts/{sctId}")]
        public async Task<ActionResult> SimpleSctEdit(int metId, int metItemId, int sctId, SimpleUpdate update)
        {
            var o = _db.METScts.FirstOrDefault(x => x.Id == sctId);
            if (o != null)
            {
                o.Name = update.Name;
                o.Index = update.Index;
                _db.METScts.Update(o);
            }

            await _db.SaveChangesAsync();

            return Ok();
        }

        public class SimpleUpdate
        {
            public string Name { get; set; }
            public int Index { get; set; }
        }
    }
}