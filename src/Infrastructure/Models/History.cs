// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Seer.Infrastructure.Models
{
    public class History
    {
        [Table("history")]
        public class HistoryItem
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [ForeignKey("UserId")]
            public Guid UserId { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public HistoryKey Key { get; set; }
            public string Value { get; set; }
            public DateTime Created { get; set; } = DateTime.UtcNow;

            public enum HistoryKey
            {
                LocalLogin = 0,
                // PctcLogin = 1,
                // ApiLogin = 2,
                // OpenIdLogin = 3,
                LogOff = 5,

                AssessmentSetActive = 10,
                AssessmentSetInactive = 11
            }
        }
    }
}