// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Seer.Infrastructure.Enums
{
    public static class EnumExtensions
    {
        public static string DisplayName(this Enum value)
        {
            return value.GetType()
                .GetMember(value.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.Name;
        }
    }
}