using ASID.Edge.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ASID.Edge.Helpers
{
    /// <summary>
    /// Parses the planner's production-plan Excel workbook.
    ///
    /// Expected layout (first worksheet):
    ///   Row  2, Col G  -> week start date
    ///   Row  2, Col Y  -> week end date
    ///   Rows 5-100:
    ///     Col B -> Serial Production  (Model)
    ///     Col D -> PU Body PN         (Part Number)
    ///     Col E -> Rev 0              (Demand quantity)
    /// </summary>
    public static class ExcelImporter
    {
        private const int WeekStartCol = 7;   // G
        private const int WeekEndCol = 25;    // Y
        private const int DateRow = 2;

        private const int FirstDataRow = 5;
        private const int LastDataRow = 100;

        private const int ModelCol = 2;   // B
        private const int PartNoCol = 4;  // D
        private const int DemandCol = 5;  // E

        public class ParseResult
        {
            public List<DailyDemand> Demands { get; set; } = new();
            public string WorkweekLabel { get; set; } = "";
            public DateTime WeekStart { get; set; }
            public DateTime WeekEnd { get; set; }
        }

        public static ParseResult Parse(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ASID");

            using var package = new ExcelPackage(new FileInfo(filePath));

            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null || worksheet.Dimension == null)
            {
                return new ParseResult();
            }

            DateTime weekStart = ParseDateCell(worksheet.Cells[DateRow, WeekStartCol]);
            DateTime weekEnd = ParseDateCell(worksheet.Cells[DateRow, WeekEndCol]);
            string workweekLabel = ComputeWorkweekLabel(weekStart, weekEnd);

            var demands = new List<DailyDemand>();

            for (int row = FirstDataRow; row <= LastDataRow; row++)
            {
                string model = worksheet.Cells[row, ModelCol].Text.Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                string partNo = worksheet.Cells[row, PartNoCol].Text.Trim();
                if (string.IsNullOrWhiteSpace(partNo))
                    partNo = model;

                string demandText = worksheet.Cells[row, DemandCol].Text.Trim();
                if (string.IsNullOrWhiteSpace(demandText) || demandText == "-")
                    continue;

                if (!int.TryParse(demandText, out int demand))
                    continue;

                demands.Add(new DailyDemand
                {
                    ProductionDate = weekStart,
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
                WorkweekLabel = workweekLabel,
                WeekStart = weekStart,
                WeekEnd = weekEnd
            };
        }

        private static DateTime ParseDateCell(ExcelRange cell)
        {
            if (cell?.Value == null)
                return DateTime.Today;

            if (cell.Value is double dVal)
                return DateTime.FromOADate(dVal);

            if (cell.Value is DateTime dtVal)
                return dtVal;

            if (DateTime.TryParse(cell.Text, out DateTime parsed))
                return parsed;

            return DateTime.Today;
        }

        public static string ComputeWorkweekLabel(DateTime weekStart, DateTime weekEnd)
        {
            int weekNumber = ISOWeek.GetWeekOfYear(weekStart);
            return $"WW{weekNumber}";
        }
    }
}