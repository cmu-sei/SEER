// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Seer.Infrastructure.Enums;

namespace Seer.Infrastructure.Models
{
    [Table("faqs")]
    public class Faq
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public ActiveStatus Status { get; set; }

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }
    }
}