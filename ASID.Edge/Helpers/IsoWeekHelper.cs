using System;
using System.Globalization;

namespace ASID.Edge.Helpers
{
    public static class IsoWeekHelper
    {
        /// <summary>
        /// Returns true when the given date falls in the same ISO week (year + week)
        /// as DateTime.Today. Returns false for null values.
        /// </summary>
        public static bool IsInCurrentWeek(DateTime? value)
        {
            if (!value.HasValue)
                return false;

            var today = DateTime.Today;
            return ISOWeek.GetYear(value.Value) == ISOWeek.GetYear(today)
                && ISOWeek.GetWeekOfYear(value.Value) == ISOWeek.GetWeekOfYear(today);
        }

        /// <summary>
        /// Business week label = ISO week number + 1 (e.g. 2026-09-14, ISO W38 → "W39").
        /// </summary>
        public static string GetBusinessWeekLabel(DateTime date) =>
            $"W{ISOWeek.GetWeekOfYear(date) + 1}";
    }
}