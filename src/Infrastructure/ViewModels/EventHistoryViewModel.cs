// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.Collections.Generic;
using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.ViewModels
{
    public class EventHistoryAssociation
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int EventDetailId { get; set; }
        public int AssessmentId { get; set; }
    }

    public class EventHistoryTableItem
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int EventDetailId { get; set; }
        public int AssessmentId { get; set; }
        public DateTime Created { get; set; }
        public string Message { get; set; }
        public string Tags { get; set; }
        public EventDetailHistory.EventHistoryStatus Status { get; set; }
    }

    public class EventHistoryTableItemWithEventAndMet
    {
        public List<MET> METs { get; set; }
        public Event Event { get; set; }
        public List<EventHistoryTableItem> Histories { get; set; } = new();
    }

    public class EventHistoryStatusUpdate
    {
        public int Id { get; set; }
        public EventDetailHistory.EventHistoryStatus Status { get; set; }
    }

    public class EventScoreUpdate
    {
        public int EventId { get; set; }
        public int? ScoreDiscovery { get; set; }
        public int? ScoreRemoval { get; set; }

        public int? ScoreSeverity { get; set; }
        public int? ScoreIntelligence { get; set; }
    }
}