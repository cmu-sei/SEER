// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.ViewModels
{
    public class HomeViewModel
    {
        public IList<Group> Groups { get; set; } = new List<Group>();
    }
}