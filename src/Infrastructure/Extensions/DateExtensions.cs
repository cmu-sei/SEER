// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using System;

namespace Seer.Infrastructure.Extensions
{
    public static class DateExtensions
    {
        /// <summary>
        /// Java timestamp is milliseconds past epoch
        /// </summary>
        public static DateTime FromJavaTimeStampToDateTime(this double javaTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime FromJavaTimeStampToDateTime(this long javaTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Provides a java based milliseconds timestamp as is used in TheHive
        /// </summary>
        public static long FromDateTimeToJavaMillisecondsTimestamp(this DateTime date)
        {
            var baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(date.ToUniversalTime() - baseDate).TotalMilliseconds;
        }
    }
}