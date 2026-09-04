using System;
using System.Linq;
using ASID.Edge.Models;
using ASID.Edge.Repositories;

namespace ASID.Edge.Services
{
    public class KanbanParser
    {
        public KanbanData Parse(string qr)
        {
            var p = qr.Split('|');
            var partNo = (p.ElementAtOrDefault(1) ?? "").TrimStart('P');

            return new KanbanData
            {
                ProductionLoop = p.ElementAtOrDefault(0) ?? "",
                
                Model = ResolveModel(partNo),

                PartNo = partNo,

                Quantity = ParseQty(p.ElementAtOrDefault(2)),

                KanbanNo = (p.ElementAtOrDefault(3) ?? "")
                    .TrimStart('S'),

                Supplier = p.ElementAtOrDefault(5) ?? "",

                Customer = p.ElementAtOrDefault(10) ?? "",

                Branch = p.ElementAtOrDefault(11) ?? "",

                Remarks = p.ElementAtOrDefault(14) ?? ""
            };
        }

        private string ResolveModel(string partNo)
        {
            if (string.IsNullOrWhiteSpace(partNo))
                return string.Empty;

            try
            {
                // 1. Check in-memory DailyDemand list if populated
                var uiMatch = RepositoryProvider.DailyDemand?
                    .FirstOrDefault(d => string.Equals(d.PartNo, partNo, StringComparison.OrdinalIgnoreCase));
                if (uiMatch != null && !string.IsNullOrWhiteSpace(uiMatch.Model))
                {
                    return uiMatch.Model;
                }

                // 2. Query DailyDemands repository
                var dbDemands = RepositoryProvider.DailyDemands?.GetAll();
                var dbMatch = dbDemands?
                    .FirstOrDefault(d => string.Equals(d.PartNo, partNo, StringComparison.OrdinalIgnoreCase));
                if (dbMatch != null && !string.IsNullOrWhiteSpace(dbMatch.Model))
                {
                    return dbMatch.Model;
                }
            }
            catch
            {
                // Fallback
            }

            return string.Empty;
        }

        private int ParseQty(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            value = value.TrimStart('Q');

            return int.TryParse(value, out int qty)
                ? qty
                : 0;
        }
    }
}
