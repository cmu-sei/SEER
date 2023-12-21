// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Seer.Infrastructure.Enums;

namespace Seer.Infrastructure.Models
{
    [Table("assessments")]
    public class Assessment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public ActiveStatus Status { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [ForeignKey("GroupId")]
        public int GroupId { get; set; }

        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")] public bool HasAnnouncements { get; set; } = true;
        public bool HasFaqs { get; set; } = true;
        public bool HasDocuments { get; set; } = true;
        public bool HasIntel { get; set; } = true;
        public bool HasOrders { get; set; } = true;
        public bool HasUploads { get; set; } = true;
        
        public IList<Document> Documents { get; set; } = new List<Document>();
        public IList<Upload> Uploads { get; set; } = new List<Upload>();
        public IList<TaskingItem> Tasks { get; set; } = new List<TaskingItem>();

        public IList<MET> METs { get; set; } = new List<MET>();

        public DateTime ExecutionTime { get; set; }
        [ForeignKey("OperationId")]
        public int OperationId { get; set; }

        public IList<Event> Events { get; private set; } = new List<Event>();

        [NotMapped]
        public class Config
        {
            public bool HasAnnouncements { get; set; }
            public bool HasFaqs { get; set; }
            public bool HasDocuments { get; set; }
            public bool HasIntel { get; set; }
            public bool HasOrders { get; set; }
            public bool HasUploads { get; set; }

            public Config() { }

            public Config(Assessment assessment)
            {
                this.HasAnnouncements = assessment.HasAnnouncements;
                this.HasDocuments = assessment.HasDocuments;
                this.HasFaqs = assessment.HasFaqs;
                this.HasIntel = assessment.HasIntel;
                this.HasOrders = assessment.HasOrders;
                this.HasUploads = assessment.HasUploads;
            }
        }
    }
}