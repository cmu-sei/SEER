// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Seer.Infrastructure.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ActiveStatus
    {
        Inactive = 0,
        Active = 1
    }
}