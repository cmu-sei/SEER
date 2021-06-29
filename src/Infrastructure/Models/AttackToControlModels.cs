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
using Newtonsoft.Json;

namespace Seer.Infrastructure.Models
{
    public class MitigationControlsRepository
    {
        public string id { get; set; }
        [JsonProperty("objects")] public IEnumerable<MitigationControl> MitigationControls { get; set; }
        public string spec_version { get; set; }
        public string type { get; set; }

        public class MitigationControl
        {
            public DateTime created { get; set; }
            public string description { get; set; }
            public IEnumerable<ExternalReferenceForMitigationControls> external_references { get; set; }
            public string id { get; set; }
            public DateTime modified { get; set; }
            public string name { get; set; }
            public string type { get; set; }

            public IEnumerable<AttackTechniquesRepository.AttackTechnique> AssociatedAttackTechniques { get; set; } =
                new List<AttackTechniquesRepository.AttackTechnique>();

            public class ExternalReferenceForMitigationControls
            {
                public string external_id { get; set; }
                public string source_name { get; set; }
            }
        }
    }

    public class MapItemsRepository
    {
        public string id { get; set; }
        [JsonProperty("objects")] public IEnumerable<MapItem> Maps { get; set; }
        public string spec_version { get; set; }
        public string type { get; set; }

        public class MapItem
        {
            public DateTime created { get; set; }
            public string id { get; set; }
            public DateTime modified { get; set; }
            public string relationship_type { get; set; }
            public string source_ref { get; set; }
            public string target_ref { get; set; }
            public string type { get; set; }
        }
    }

    public class AttackTechniquesRepository
    {
        public string id { get; set; }

        [JsonProperty("objects")] public IEnumerable<AttackTechnique> AttackTechniques { get; set; }
        public string spec_version { get; set; }
        public string type { get; set; }

        public class AttackTechnique
        {
            public DateTime created { get; set; }
            public IEnumerable<ExternalReferenceForAttackTechniques> external_references { get; set; }
            public string id { get; set; }
            public DateTime modified { get; set; }
            public string name { get; set; }
            public bool revoked { get; set; }
            public string type { get; set; }

            public IEnumerable<MitigationControlsRepository.MitigationControl> AssociatedMitigationControls { get; set; } =
                new List<MitigationControlsRepository.MitigationControl>();

            public class ExternalReferenceForAttackTechniques
            {
                public string external_id { get; set; }
                public string source_name { get; set; }
                public string url { get; set; }
                public string description { get; set; }
            }
        }
    }

    public class MitigationControls
    {
        public IEnumerable<MitigationControlsRepository.MitigationControl> Records { get; set; } =
            new List<MitigationControlsRepository.MitigationControl>();

        public int Count { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }

    public class AttackTechniques
    {
        public IEnumerable<AttackTechniquesRepository.AttackTechnique> Records { get; set; } =
            new List<AttackTechniquesRepository.AttackTechnique>();

        public int Count { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}