// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Seer.Infrastructure.Enums;

namespace Seer.Infrastructure.Models
{
    public enum MetType {
        MET = 0,
        JMET = 10
    }
    
    /// <summary>
    /// Mission Essential Tasks
    /// </summary>
    [Table("mets")]
    public class MET
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public IList<METItem> METItems { get; set; } = new List<METItem>();

        [ForeignKey("AssessmentId")]
        public int AssessmentId { get; set; }
        
        public MetType Type {get; set;}
    }

    [Table("met_items")]
    public class METItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [ForeignKey("MetId")]
        public int MetId { get; set; }

        public IList<METItemSCT> METSCTs { get; set; } = new List<METItemSCT>();
    }

    [Table("met_item_scts")]
    public class METItemSCT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Index { get; set; }
        
        /// <summary>
        /// This gets displayed, could also be referred to as "type" - most commonly "SCT" or "Step"
        /// </summary>
        public string Title { get; set; }
        
        public string Name { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public int SectionId { get; set; }
        public METItemSCTScore Score { get; set; }
        public ActiveStatus Status { get; set; }

        [ForeignKey("MetItemId")]
        public int MetItemId { get; set; }
    }

    [Table("met_item_sct_scores")]
    public class METItemSCTScore
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Score SCTScore { get; set; }
        public int METId { get; set; }
        public int SCTId { get; set; }
        
        public int AssessmentEventId { get; set; }
        public string Comments { get; set; }
        public string UserId { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public enum Score
        {
            Go = 1,
            [System.ComponentModel.Description("No go")]
            NoGo = 2,
            Partial = 3,
            [System.ComponentModel.Description("N/A")]
            NA = 9
        }
    }
}