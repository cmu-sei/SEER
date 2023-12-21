// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

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