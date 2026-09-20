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

        /// <summary>
        /// Returns the Monday (date only) of the ISO week containing <paramref name="date"/>.
        /// Used as the canonical week key for daily demand rows.
        /// </summary>
        public static DateTime GetWeekStart(DateTime date)
        {
            int daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
            return date.Date.AddDays(-daysFromMonday);
        }

        /// <summary>
        /// Returns true when the given date falls in the same week as <paramref name="weekStart"/>
        /// (a Monday). Returns false for null values.
        /// </summary>
        public static bool IsInWeek(DateTime? value, DateTime weekStart)
        {
            if (!value.HasValue)
                return false;

            return GetWeekStart(value.Value) == GetWeekStart(weekStart);
        }

        /// <summary>
        /// Dual-convention display label: business week (ISO + 1) followed by the raw ISO week,
        /// e.g. 2026-09-14 → "W39 (ISO W38)". Display-only; no stored date changes.
        /// </summary>
        public static string GetWeekDisplayLabel(DateTime weekStart)
        {
            int isoWeek = ISOWeek.GetWeekOfYear(weekStart);
            return $"{GetBusinessWeekLabel(weekStart)} (ISO W{isoWeek})";
        }
    }
}