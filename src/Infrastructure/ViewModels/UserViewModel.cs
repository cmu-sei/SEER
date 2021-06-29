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