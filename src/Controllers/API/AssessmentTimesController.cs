// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Services;

namespace Seer.Controllers.API
{
    [AllowAnonymous]
    [Route("api/assessments/times")]
    public class AssessmentTimesController : BaseController
    {
        public AssessmentTimesController(ApplicationDbContext dbContext)
        {
            this._db = dbContext;
        }

        [HttpGet("{assessmentId}")]
        [ProducesResponseType(typeof(AssessmentTime), (int)HttpStatusCode.OK)]
        public async Task<AssessmentTime> Index(int assessmentId)
        {
            var assessmentTimeService = new AssessmentTimeService(this._db);
            await assessmentTimeService.Get(assessmentId);
            return assessmentTimeService.Time;
        }
    }
}