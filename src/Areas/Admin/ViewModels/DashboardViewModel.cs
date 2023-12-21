// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using Seer.Infrastructure.Models;

namespace Seer.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public IList<Campaign> Campaigns { get; set; } = new List<Campaign>();
        public IList<Group> Groups { get; set; } = new List<Group>();
    }
}