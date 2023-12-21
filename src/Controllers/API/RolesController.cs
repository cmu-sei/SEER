// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Seer.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace Seer.Controllers.API
{
    [Route("api/roles/")]
    public class RolesController : BaseController
    {
        [HttpGet]
        public IEnumerable<User.Role> Index()
        {
            return Enum.GetValues(typeof(User.Role)).Cast<User.Role>().ToList();
        }
    }
}