using ASID.Edge.Models;
using System;

namespace ASID.Edge.Services
{
    public class DataMatrixParser
    {
        public DataMatrixData Parse(string barcode)
        {
            if (!barcode.StartsWith("ASID1"))
                throw new Exception("Invalid ASID Data Matrix.");

            var data = new DataMatrixData();

            data.TransactionId =
                Read(barcode, "T", "P");

            data.PartNo =
                Read(barcode, "P", "K");

            data.KanbanNo =
                Read(barcode, "K", "Q");

            data.Quantity =
                int.Parse(Read(barcode, "Q", "M"));

            data.Model =
                Read(barcode, "M", "L");

            data.Location =
                Read(barcode, "L", "D");

            data.Timestamp =
                DateTime.ParseExact(
                    Read(barcode, "D", null),
                    "yyyyMMddHHmmss",
                    null);

            return data;
        }

        private string Read(
            string source,
            string startTag,
            string? endTag)
        {
            int start =
                source.IndexOf(startTag);

            if (start < 0)
                return "";

            start += startTag.Length;

            if (endTag == null)
                return source[start..];

            int end =
                source.IndexOf(endTag, start);

            if (end < 0)
                return source[start..];

            return source[start..end];
        }
    }
}