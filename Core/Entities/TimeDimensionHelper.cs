#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Entities
{
    public class TimeDimensionHelper
    {
        private static readonly string[] WeeklyKeys = { "fc_week", "fc_week_to_period" };
        private static readonly string[] PeriodKeys = { "fc_period" };
        private static readonly string[] RollingKeys = { "roll4", "roll13", "week26", "week52" };

        private static readonly Dictionary<TimePeriodType, string> TimePeriodMap = new()
        {
            { TimePeriodType.Roll4, "roll4" },
            { TimePeriodType.Roll13, "roll13" },
            { TimePeriodType.Week26, "week26" },
            { TimePeriodType.Week52, "week52" }
        };

        public static string[] GetAllDimensionKeys()
        {
            var allKeys = new List<string>();
            allKeys.AddRange(WeeklyKeys);
            allKeys.AddRange(PeriodKeys);
            allKeys.AddRange(RollingKeys);
            return allKeys.ToArray();
        }

        public static string? GetTimeDimensionKey(KpiRequest request)
        {
            var timeAttrs = request.FilterBy?.TimeAttributes;
            if (timeAttrs == null) return null;

            // Check Fc week with potential grouping by period
            if (timeAttrs.FcWeek.HasValue)
            {
                var groupByFcPeriod = request.GroupBy?.Contains("fc_period", StringComparer.OrdinalIgnoreCase) ?? false;
                return groupByFcPeriod ? "fc_week_to_period" : "fc_week";
            }

            // Check Fc period
            if (timeAttrs.FcPeriod.HasValue)
            {
                return "fc_period";
            }

            // Check rolling periods
            if (timeAttrs.TimePeriod.HasValue && 
                TimePeriodMap.TryGetValue(timeAttrs.TimePeriod.Value, out var periodKey))
            {
                return periodKey;
            }

            return null;
        }

        public static bool IsRollingPeriod(string dimensionKey)
        {
            return Array.Exists(RollingKeys, key => key.Equals(dimensionKey, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsWeeklyDimension(string dimensionKey)
        {
            return Array.Exists(WeeklyKeys, key => key.Equals(dimensionKey, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsPeriodDimension(string dimensionKey)
        {
            return Array.Exists(PeriodKeys, key => key.Equals(dimensionKey, StringComparison.OrdinalIgnoreCase));
        }
    }
}