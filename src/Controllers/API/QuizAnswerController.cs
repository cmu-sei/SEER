/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seer.Infrastructure.Data;

namespace Seer.Controllers.API
{
    [Route("api/quiz/")]
    public class QuizAnswerController : BaseController
    {
        public QuizAnswerController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpGet]
        [Route("questions/answers")]
        [ProducesResponseType(typeof(IEnumerable<QuizAnswer>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _db.Answers.OrderByDescending(o => o.Created).ToListAsync(ct);
            return Ok(list);
        }

        [HttpGet]
        [Route("questions/{questionId}/answers")]
        [ProducesResponseType(typeof(IEnumerable<QuizAnswer>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(int questionId, CancellationToken ct)
        {
            var list = await _db.Answers.Where(o => o.QuestionId == questionId).OrderByDescending(o => o.Created).ToListAsync(ct);
            return Ok(list);
        }

        [HttpPost]
        [Route("questions/{questionId}/answers")]
        [ProducesResponseType(typeof(QuizAnswer), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Create(int questionId, QuizAnswer answer, CancellationToken ct)
        {
            answer.QuestionId = questionId;
            await _db.Answers.AddAsync(answer, ct);
            await _db.SaveChangesAsync(ct);
            return Ok(answer);
        }

        [HttpPut]
        [Route("questions/{questionId}/answers/{answerId}")]
        [ProducesResponseType(typeof(QuizAnswer), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update(int questionId, int answerId, QuizAnswer answer, CancellationToken ct)
        {
            var item = await _db.Answers.FirstOrDefaultAsync(o => o.QuestionId == questionId && o.Id == answerId, ct);
            if (item == null)
            {
                return NotFound();
            }

            // only certain values can be changed
            item.Status = answer.Status;
            item.UserId = answer.UserId;
            item.AnsweredText = answer.AnsweredText;
            item.AnsweredIndex = answer.AnsweredIndex;
            item.HintTakenBy = answer.HintTakenBy;

            _db.Answers.Update(item);
            await _db.SaveChangesAsync(ct);
            return Ok(answer);
        }

        [HttpDelete]
        [Route("questions/{questionId}/answers/{answerId}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(int questionId, int answerId, CancellationToken ct)
        {
            var item = await _db.Answers.FirstOrDefaultAsync(o => o.QuestionId == questionId && o.Id == answerId, ct);
            if (item == null)
            {
                return NotFound();
            }

            _db.Answers.Remove(item);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}