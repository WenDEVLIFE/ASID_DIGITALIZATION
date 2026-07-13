using ASID.Edge.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Services
{
    public class DataMatrixService
    {

        public string Generate(DataMatrixData data)
        {
            return
                $"ASID1" +
                $"T{data.TransactionId}" +
                $"P{data.PartNo}" +
                $"K{data.KanbanNo}" +
                $"Q{data.Quantity}" +
                $"M{Normalize(data.Model)}" +
                $"L{data.Location}";
               // + $"D{data.Timestamp:yyyyMMddHHmmss}";
        }
        private string Normalize(string text)
        {
            return text
                .Replace(" ", "")
                .Replace("-", "")
                .ToUpper();
        }

    }
}
