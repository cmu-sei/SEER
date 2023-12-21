// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FileHelpers;
using Seer.Infrastructure.Enums;

namespace Seer.Infrastructure.Models
{
    [Table("groups")]
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ActiveStatus Status { get; set; } = ActiveStatus.Active;
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Brigade { get; set; }
        public string Theatre { get; set; }
        public string SignalCorp { get; set; }

        public IList<Assessment> Assessments { get; set; } = new List<Assessment>();
        public IList<GroupUser> Users { get; set; } = new List<GroupUser>();

        public Group() { }

        public Group(GroupImport import)
        {
            if (import.Id.HasValue)
                this.Id = import.Id.Value;
            this.Brigade = import.Brigade.Replace("\"", string.Empty).Trim();
            this.Designation = import.Designation.Replace("\"", string.Empty).Trim();
            this.Name = import.Name.Replace("\"", string.Empty).Trim();
            this.SignalCorp = import.SignalCorp.Replace("\"", string.Empty).Trim();
            this.Theatre = import.Theatre.Replace("\"", string.Empty).Trim();
        }
    }

    [Table("group_users")]
    public class GroupUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string UserId { get; set; }
    }

    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    public class GroupImport
    {
        public int? Id;
        public string Name;
        public string Designation;
        public string Brigade;
        public string Theatre;
        public string SignalCorp;
    }
}