/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

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