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
        /// Display label for the production workweek column: the week number as it appears in
        /// the planner's Excel file, e.g. the Monday of ISO W41 → "W41".
        ///
        /// The plan's "Work Week" column is interpreted as the ISO week
        /// (see ExcelImporter.GetDateFromWeekNumber), so the dashboard prints the SAME number
        /// the planner typed — there is deliberately NO +1 offset (the old
        /// "business week = ISO + 1" label printed W42 for a plan week of W41).
        /// Display-only; no stored date changes.
        /// </summary>
        public static string GetWeekDisplayLabel(DateTime weekStart) =>
            $"W{ISOWeek.GetWeekOfYear(weekStart)}";
    }
}