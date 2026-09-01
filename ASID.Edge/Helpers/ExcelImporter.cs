using ASID.Edge.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASID.Edge.Helpers
{
    /// <summary>
    /// Parses the planner's production-plan Excel workbook.
    ///
    /// Searches all worksheets for one containing "Work Week" or "Serial Production"
    /// in its header rows, then reads data from the matching sheet.
    ///
    /// Expected layout (e.g. "Body Supply" sheet):
    ///   Row 2-4: Headers (Work Week, Serial Production, PU Body PN, Rev. 0)
    ///   Row 5+: Data rows
    ///     Col A -> Work Week number (e.g. 33)
    ///     Col B -> Serial Production  (Model Name)
    ///     Col D -> PU Body PN         (Part Number)
    ///     Col E -> Rev 0              (Demand quantity)
    /// </summary>
    public static class ExcelImporter
    {
        public class ParseResult
        {
            public List<DailyDemand> Demands { get; set; } = new();
            public string WorkweekLabel { get; set; } = "";
        }

        public static ParseResult Parse(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ASID");

            using var package = new ExcelPackage(new FileInfo(filePath));

            // Find the worksheet that contains "Work Week" or "Serial Production" headers
            ExcelWorksheet? worksheet = null;
            for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
            {
                var ws = package.Workbook.Worksheets[i];
                if (HasHeader(ws, "Work Week") || HasHeader(ws, "Serial Production"))
                {
                    worksheet = ws;
                    break;
                }
            }

            if (worksheet == null || worksheet.Dimension == null)
            {
                return new ParseResult();
            }

            // Find the data start row (first row where Col A has a numeric workweek)
            int firstDataRow = FindDataStartRow(worksheet);

            // Read workweek from Column A (first non-empty value)
            // Convert week number to actual date (Monday of that week)
            string workweekLabel = "";
            int workweekNumber = 0;
            for (int row = firstDataRow; row <= worksheet.Dimension.Rows; row++)
            {
                string ww = worksheet.Cells[row, 1].Text.Trim();
                if (!string.IsNullOrWhiteSpace(ww) && int.TryParse(ww, out int wwNum))
                {
                    workweekNumber = wwNum;
                    workweekLabel = $"WW {ww}";
                    break;
                }
            }

            // Find model and part number columns (first match in header rows)
            int modelCol = FindColumn(worksheet, 2, 4, "Serial Production");
            int partNoCol = FindColumn(worksheet, 2, 4, "PU Body PN");

            // For demand column: find ALL "Rev" columns and pick the one with actual data
            int demandCol = FindDemandColumn(worksheet, firstDataRow);

            // Fallbacks
            if (modelCol <= 0) modelCol = 2;  // B
            if (partNoCol <= 0) partNoCol = 4; // D
            if (demandCol <= 0) demandCol = 5;  // E

            var demands = new List<DailyDemand>();

            for (int row = firstDataRow; row <= worksheet.Dimension.Rows; row++)
            {
                string model = worksheet.Cells[row, modelCol].Text.Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                string partNo = worksheet.Cells[row, partNoCol].Text.Trim();
                if (string.IsNullOrWhiteSpace(partNo))
                    partNo = model;

                string demandText = worksheet.Cells[row, demandCol].Text.Trim();
                if (string.IsNullOrWhiteSpace(demandText) || demandText == "-")
                    demandText = "0";

                if (!int.TryParse(demandText, out int demand))
                    demand = 0;

                // Include row even if demand is 0 (planner may update later)
                demands.Add(new DailyDemand
                {
                    ProductionDate = GetDateFromWeekNumber(workweekNumber),
                    Shift = 0,
                    Model = model,
                    PartNo = partNo,
                    Quantity = demand,
                    Scrapped = 0,
                    ImportedAt = DateTime.UtcNow
                });
            }

            return new ParseResult
            {
                Demands = demands,
                WorkweekLabel = workweekLabel
            };
        }

        /// <summary>
        /// Find the demand column by checking ALL columns that contain "Rev" in headers,
        /// then picking the one that actually has numeric data in the data rows.
        /// This handles the case where multiple "Rev. 0" columns exist but only one has data.
        /// </summary>
        private static int FindDemandColumn(ExcelWorksheet ws, int firstDataRow)
        {
            if (ws.Dimension == null) return -1;

            // Find all columns with "Rev" in header rows (2-4)
            var revColumns = new List<int>();
            int maxCol = Math.Min(ws.Dimension.Columns, 40);
            for (int row = 2; row <= 4; row++)
            {
                for (int col = 1; col <= maxCol; col++)
                {
                    if (ws.Cells[row, col].Text.Trim()
                        .Contains("Rev", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!revColumns.Contains(col))
                            revColumns.Add(col);
                    }
                }
            }

            if (revColumns.Count == 0) return -1;
            if (revColumns.Count == 1) return revColumns[0];

            // Multiple "Rev" columns found — pick the one with actual numeric data
            int bestCol = revColumns[0];
            int bestScore = 0;

            foreach (var col in revColumns)
            {
                int score = 0;
                for (int row = firstDataRow; row <= Math.Min(ws.Dimension.Rows, firstDataRow + 20); row++)
                {
                    string val = ws.Cells[row, col].Text.Trim();
                    if (!string.IsNullOrWhiteSpace(val) && val != "-" &&
                        int.TryParse(val, out int num) && num > 0)
                    {
                        score++;
                    }
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCol = col;
                }
            }

            return bestCol;
        }

        /// <summary>
        /// Check if a worksheet has the given header text in any cell in rows 1-5.
        /// </summary>
        private static bool HasHeader(ExcelWorksheet ws, string headerText)
        {
            if (ws.Dimension == null) return false;

            int maxCol = Math.Min(ws.Dimension.Columns, 10);
            for (int row = 1; row <= 5; row++)
            {
                for (int col = 1; col <= maxCol; col++)
                {
                    if (ws.Cells[row, col].Text.Trim()
                        .Contains(headerText, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Find the first row where Column A contains a numeric value (workweek number).
        /// </summary>
        private static int FindDataStartRow(ExcelWorksheet ws)
        {
            if (ws.Dimension == null) return 5;

            for (int row = 1; row <= Math.Min(ws.Dimension.Rows, 20); row++)
            {
                string val = ws.Cells[row, 1].Text.Trim();
                if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out _))
                    return row;
            }

            return 5; // default
        }

        /// <summary>
        /// Search header rows for a column containing the given text.
        /// </summary>
        private static int FindColumn(ExcelWorksheet ws, int headerStartRow, int headerEndRow, string headerText)
        {
            if (ws.Dimension == null) return -1;

            int maxCol = Math.Min(ws.Dimension.Columns, 40);
            for (int row = headerStartRow; row <= headerEndRow; row++)
            {
                for (int col = 1; col <= maxCol; col++)
                {
                    if (ws.Cells[row, col].Text.Trim()
                        .Contains(headerText, StringComparison.OrdinalIgnoreCase))
                    {
                        return col;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Convert an ISO week number to the Monday date of that week.
        /// Uses the current year. If the week number is out of range,
        /// falls back to today's date.
        /// </summary>
        private static DateTime GetDateFromWeekNumber(int weekNumber)
        {
            if (weekNumber < 1 || weekNumber > 53)
                return DateTime.Today;

            // ISO 8601: Week 1 is the week containing the first Thursday of the year.
            // January 4th is always in Week 1.
            var jan4 = new DateTime(DateTime.Today.Year, 1, 4);
            int startOfWeek1 = (int)jan4.DayOfWeek;
            if (startOfWeek1 == 0) startOfWeek1 = 7; // Sunday = 7
            var week1Monday = jan4.AddDays(-(startOfWeek1 - 1));

            // Monday of the target week
            var targetMonday = week1Monday.AddDays((weekNumber - 1) * 7);

            // Sanity check: don't go more than 7 days before or after today
            if (targetMonday.Year != DateTime.Today.Year)
                return DateTime.Today;

            return targetMonday;
        }
    }
}
