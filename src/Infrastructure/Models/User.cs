// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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