// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using Seer.Infrastructure.Models;

namespace Seer.Areas.Admin.ViewModels
{
    public class CatalogViewModel
    {
        public IList<AssessmentEventCatalogItem> CatalogItems { get; set; } = new List<AssessmentEventCatalogItem>();
    }
}