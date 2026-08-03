using ASID.Edge.Models;
using OfficeOpenXml;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ASID.Edge.Helpers
{
    public static class ExcelImporter
    {
        private const int DateRow = 2;
        private const int ShiftRow = 4;
        private const int FirstDataRow = 5;

        private const int ModelColumn = 2;
        private const int FirstDemandColumn = 5;

        public static List<DailyDemand> Parse(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("ASID");

            using var package = new ExcelPackage(new FileInfo(filePath));

            var worksheet = package.Workbook.Worksheets["Body Supply"];

            var result = new List<DailyDemand>();

            int lastRow = worksheet.Dimension.End.Row;
            int lastColumn = worksheet.Dimension.End.Column;

            for (int row = FirstDataRow; row <= lastRow; row++)
            {
                string model =
                    worksheet.Cells[row, ModelColumn].Text.Trim();

                if (string.IsNullOrWhiteSpace(model))
                    continue;

                for (int col = FirstDemandColumn; col <= lastColumn; col++)
                {
                    //Read qunatity
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

                    if (!DateTime.TryParse(
                        worksheet.Cells[DateRow, dateColumn].Text,
                        out DateTime productionDate))
                    {
                        continue;
                    }

                    // Read shift
                    short shift = GetShift(
                        worksheet.Cells[ShiftRow, col].Text);

                    result.Add(new DailyDemand
                    {
                        ProductionDate = productionDate,
                        Shift = shift,
                        Model = model,
                        PartNo = model, //temporary workaround because no mapping of model and partNo
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