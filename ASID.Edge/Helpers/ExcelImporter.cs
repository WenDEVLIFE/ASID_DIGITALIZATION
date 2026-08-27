using ASID.Edge.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace ASID.Edge.Helpers
{
    /// <summary>
    /// Parses the planner's production-plan Excel workbook.
    ///
    /// Expected layout (first worksheet):
    ///   Rows 5-100:
    ///     Col A -> Work Week number (e.g. 33)
    ///     Col B -> Serial Production  (Model Name)
    ///     Col D -> PU Body PN         (Part Number)
    ///     Col E -> Rev 0              (Demand quantity)
    /// </summary>
    public static class ExcelImporter
    {
        private const int FirstDataRow = 5;
        private const int LastDataRow = 100;

        private const int WorkWeekCol = 1;  // A
        private const int ModelCol = 2;     // B
        private const int PartNoCol = 4;    // D
        private const int DemandCol = 5;    // E

        public class ParseResult
        {
            public List<DailyDemand> Demands { get; set; } = new();
            public string WorkweekLabel { get; set; } = "";
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

            // Read workweek from Column A (first non-empty value)
            string workweekLabel = "";
            for (int row = FirstDataRow; row <= LastDataRow; row++)
            {
                string ww = worksheet.Cells[row, WorkWeekCol].Text.Trim();
                if (!string.IsNullOrWhiteSpace(ww))
                {
                    workweekLabel = $"WW {ww}";
                    break;
                }
            }

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
                    ProductionDate = DateTime.Today,
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
    }
}
