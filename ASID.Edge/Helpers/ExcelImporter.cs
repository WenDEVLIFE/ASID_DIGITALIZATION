using ASID.Edge.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace ASID.Edge.Helpers
{
    public static class ExcelImporter
    {
        private const int DateRow = 2;
        private const int ShiftRow = 4;
        private const int FirstDataRow = 5;

        private const int ModelColumn = 2;
        private const int PartNoColumn = 4;
        private const int FirstDemandColumn = 6;

        public static List<DailyDemand> Parse(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ASID");

            using var package = new ExcelPackage(new FileInfo(filePath));

            var worksheet = package.Workbook.Worksheets["Body Supply"];
            if (worksheet == null || worksheet.Dimension == null)
            {
                return new List<DailyDemand>();
            }

            var result = new List<DailyDemand>();

            int lastRow = worksheet.Dimension.End.Row;
            int lastColumn = worksheet.Dimension.End.Column;

            for (int row = FirstDataRow; row <= lastRow; row++)
            {
                string model = worksheet.Cells[row, ModelColumn].Text.Trim();

                if (string.IsNullOrWhiteSpace(model))
                    continue;

                string partNo = worksheet.Cells[row, PartNoColumn].Text.Trim();
                if (string.IsNullOrWhiteSpace(partNo))
                {
                    partNo = model;
                }

                for (int col = FirstDemandColumn; col <= lastColumn; col++)
                {
                    // Read quantity
                    string quantityText = worksheet.Cells[row, col].Text.Trim();

                    int quantity = 0;

                    if (quantityText == "-" || string.IsNullOrWhiteSpace(quantityText))
                    {
                        quantity = 0;
                    }
                    else if (!int.TryParse(quantityText, out quantity))
                    {
                        continue; // Unexpected value
                    }

                    // Read production date
                    int dateColumn = col - ((col - FirstDemandColumn) % 3);
                    var dateCellVal = worksheet.Cells[DateRow, dateColumn].Value;
                    DateTime productionDate;

                    if (dateCellVal is double dVal)
                    {
                        productionDate = DateTime.FromOADate(dVal);
                    }
                    else if (dateCellVal is DateTime dtVal)
                    {
                        productionDate = dtVal;
                    }
                    else if (!DateTime.TryParse(worksheet.Cells[DateRow, dateColumn].Text, out productionDate))
                    {
                        continue;
                    }

                    // Read shift
                    short shift = GetShift(worksheet.Cells[ShiftRow, col].Text);

                    result.Add(new DailyDemand
                    {
                        ProductionDate = productionDate,
                        Shift = shift,
                        Model = model,
                        PartNo = partNo,
                        Quantity = quantity
                    });
                }
            }

            return result;
        }

        private static short GetShift(string shiftText)
        {
            shiftText = shiftText.Trim().ToLower();

            if (shiftText.StartsWith("1"))
                return 1;

            if (shiftText.StartsWith("2"))
                return 2;

            if (shiftText.StartsWith("3"))
                return 3;

            return 0;
        }
    }
}