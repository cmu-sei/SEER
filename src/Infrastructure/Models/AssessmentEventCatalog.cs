// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seer.Infrastructure.Models
{
    [Table("assessment_event_catalog_items")]
    public class AssessmentEventCatalogItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string ExtendedName { get; set; }
        public string ExecutiveSummary { get; set; }
        public string TargetedSystems { get; set; }
        public string Detection { get; set; }
        public string Storyline { get; set; }
        public string Organization { get; set; }
        public string Intel { get; set; }
        public string AnticipatedResponse { get; set; }
        public string TechnicalDetails { get; set; }
        public string Owner { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public IList<AssessmentEventCatalogItemDetail> Details { get; set; } = new List<AssessmentEventCatalogItemDetail>();

        public AssessmentEventCatalogItem(Event e)
        {
            foreach (var detail in e.Details)
            {
                this.Details.Add(new AssessmentEventCatalogItemDetail
                {
                    Name = detail.Name,
                    Procedures = detail.Procedures,
                    AttackProfile = detail.AttackProfile,
                    DisplayOrder = detail.DisplayOrder
                });
            }
        }

        public AssessmentEventCatalogItem() { }
    }

    [Table("assessment_event_catalog_item_details")]
    public class AssessmentEventCatalogItemDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int DisplayOrder { get; set; }
        public string Name { get; set; }
        public string Procedures { get; set; }
        public string AttackProfile { get; set; }

        [ForeignKey("AssessmentEventCatalogItemId")]
        public int AssessmentEventCatalogItemId { get; set; }
    }
}