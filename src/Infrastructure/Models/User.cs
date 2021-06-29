/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Seer.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Seer.Infrastructure.Models
{
    public sealed class User : IdentityUser
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Rank { get; set; }
        [Display(Name = "Duty Position")]
        public string DutyPosition { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string OAuthId { get; set; }

        [NotMapped] public IEnumerable<string> Roles { get; set; } = new List<string>();

        public User()
        {
        }

        public User(UserImport import)
        {
            if (!string.IsNullOrEmpty(import.Id))
                this.Id = import.Id;
            this.FirstName = import.FirstName.TrimImport();
            this.LastName = import.LastName.TrimImport();
            this.UserName = import.UserName.TrimImport();
            this.Email = import.Email.TrimImport();
            this.UserName = import.UserName.TrimImport();
            this.DutyPosition = import.DutyPosition.TrimImport();
            this.Rank = import.Rank.TrimImport();

            if (!string.IsNullOrEmpty(this.FirstName)) return;

            var name = this.LastName.Split(Convert.ToChar(" "));
            if (name.Length <= 1) return;

            this.FirstName = name[0];
            this.LastName = this.LastName.Replace(this.FirstName, "").Trim();
        }

        public string Name()
        {
            return $"{this.FirstName} {this.LastName}";
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Role
        {
            Player = 0,
            EO = -5,
            Admin = -9
        }
    }
}