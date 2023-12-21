// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Seer.Infrastructure.Models
{
    /// <summary>
    /// GCD 2020, GCD 2021
    /// </summary>
    [Table("campaigns")]
    public class Campaign
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public IList<Operation> Operations { get; set; } = new List<Operation>();
        public IList<CampaignDataPoint> DataPoints { get; set; } = new List<CampaignDataPoint>();
    }

    /// <summary>
    /// Communications Insights, NPC Agent Activity, etc...
    /// 
    /// Just making these strings for now, because I want to keep them unstructured and
    /// able to support a large range of data
    /// </summary>
    [Table("campaign_data_points")]
    public class CampaignDataPoint
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}