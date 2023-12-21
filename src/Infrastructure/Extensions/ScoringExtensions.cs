// Copyright 2021 Carnegie Mellon University. All Rights Reserved. See LICENSE.md file for terms.

using Seer.Infrastructure.Models;

namespace Seer.Infrastructure.Extensions
{
    public static class ScoringExtensions
    {
        public static void CalculatePercentOfMax(this TeamScore.Score s, decimal max)
        {
            if (s.Value == max)
            {
                s.Percentage = 1;
                if (max == 0)
                {
                    s.Percentage = 0;
                }
            }
            else if (s.Value == 0)
            {
                s.Percentage = 0;
            }
            else
            {
                s.Percentage = s.Value / max;
            }
        }

        public static int CalculateIntFromNullable(this int? value)
        {
            return value ?? 0;
        }
    }
}