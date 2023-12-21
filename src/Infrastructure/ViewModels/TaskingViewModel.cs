// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.ViewModels
{
    public class TaskingViewModel
    {
        public int GroupId { get; set; }
        public IList<TaskingItem> Items { get; set; } = new List<TaskingItem>();
        public IList<TaskingItemResult> Results { get; set; } = new List<TaskingItemResult>();
    }
}