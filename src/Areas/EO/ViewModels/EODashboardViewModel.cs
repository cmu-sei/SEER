// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Seer.Infrastructure.Models;

namespace Seer.Areas.EO.ViewModels
{
    [NotMapped]
    public class EODashboardViewModel
    {
        public IList<User> Users { get; set; } = new List<User>();
        public IList<MET> METs { get; set; } = new List<MET>();
        public IList<Section> Sections { get; set; } = new List<Section>();
    }
}