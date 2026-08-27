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
            string workweekLabel = "";
            for (int row = firstDataRow; row <= worksheet.Dimension.Rows; row++)
            {
                string ww = worksheet.Cells[row, 1].Text.Trim();
                if (!string.IsNullOrWhiteSpace(ww) && int.TryParse(ww, out _))
                {
                    workweekLabel = $"WW {ww}";
                    break;
                }
            }

            // Find the column indices by scanning header rows (rows 2-4)
            int modelCol = FindColumn(worksheet, 2, 4, "Serial Production");
            int partNoCol = FindColumn(worksheet, 2, 4, "PU Body PN");
            int demandCol = FindColumn(worksheet, 2, 4, "Rev");

            // If we couldn't find by header name, fall back to known positions
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
    }
}
