// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seer.Infrastructure.Models
{
    [Table("uploads")]
    public class Upload
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string UserId { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }

        [NotMapped]
        public User User { get; set; }
        [NotMapped]
        public Group Group { get; set; }
    }
}