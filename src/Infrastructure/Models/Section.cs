// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seer.Infrastructure.Models
{
    [Table("sections")]
    public class Section
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }

        public string Name { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}