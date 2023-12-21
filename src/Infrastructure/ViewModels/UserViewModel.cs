// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public string Email { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Rank { get; set; }
        [Display(Name = "Duty Position")]
        public string DutyPosition { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }

        public IList<Group> Groups { get; set; } = new List<Group>();

        public User.Role Role { get; set; }

        public UserViewModel()
        {
        }

        public UserViewModel(User user)
        {
            this.Id = user.Id;
            this.Created = user.Created;
            this.Email = user.Email;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.PhoneNumber = user.PhoneNumber;
            this.UserName = user.UserName;
            this.DutyPosition = user.DutyPosition;
            this.Rank = user.Rank;
        }
    }
}