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