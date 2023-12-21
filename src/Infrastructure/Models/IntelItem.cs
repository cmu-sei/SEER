// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Seer.Infrastructure.Enums;

namespace Seer.Infrastructure.Models
{
    [Table("intel_items")]
    public class IntelItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Subject { get; set; }
        [DataType(DataType.MultilineText)]
        public string Details { get; set; }
        [DataType(DataType.MultilineText)]
        public string Tags { get; set; }
        public DateTime Created { get; set; }
        public ActiveStatus Status { get; set; }

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }
    }
}