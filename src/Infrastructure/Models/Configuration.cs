/*
SEER - SYSTEM (for) EVENT EVALUATION RESEARCH 
Copyright 2021 Carnegie Mellon University. 
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT. 
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms. 
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution. 
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University. 
DM21-0384 
*/

using System.Collections.Generic;

namespace Seer.Infrastructure.Models
{
    public class DefaultSeedValue
    {
        public class AssessmentSeed
        {
            public string Name { get; set; }
            public int Status { get; set; }
        }

        public class GroupSeed
        {
            public string Group { get; set; }
            public IEnumerable<AssessmentSeed> Assessments { get; set; }
        }

        public string Campaign { get; set; }
        public IEnumerable<string> Operations { get; set; }
        public IEnumerable<GroupSeed> Groups { get; set; }
    }

    /// <summary>
    /// application configuration
    /// </summary>
    public class Configuration
    {
        public string TimeZone { get; set; }
        public string Version { get; set; }
        public string UserSalt { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public string AuthenticationAuthority { get; set; }
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }
        public string[] DefaultAdminAccounts { get; set; }

        public DatabaseOptions Database { get; set; }

        public string Host { get; set; }

        public class DatabaseOptions
        {
            public string Provider { get; set; }
            public string ConnectionString { get; set; }
            public string SeedFile { get; } = "seed.json";
        }
    }
}