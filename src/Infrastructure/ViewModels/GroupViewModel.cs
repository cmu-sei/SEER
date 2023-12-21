// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.ViewModels
{
    public class GroupViewModel
    {
        public Group Group { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();

        public GroupViewModel() { }

        public GroupViewModel(Group g)
        {
            this.Group = g;
            this.Assessments = g.Assessments;
        }
    }

    public class AssessmentAndGroup
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
}