using ASID.Edge.Models;

namespace ASID.Edge.Services
{
    public class KanbanParser
    {
        public KanbanData Parse(string qr)
        {
            var p = qr.Split('|');

            return new KanbanData
            {
                ProductionLoop = p.ElementAtOrDefault(0) ?? "",
                
                Model = "PU BODY", //change this later to get from datbase

                PartNo = (p.ElementAtOrDefault(1) ?? "").TrimStart('P'),

                Quantity = ParseQty(p.ElementAtOrDefault(2)),

                KanbanNo = (p.ElementAtOrDefault(3) ?? "")
                    .TrimStart('S'),

                Supplier = p.ElementAtOrDefault(5) ?? "",

                Customer = p.ElementAtOrDefault(10) ?? "",

                Branch = p.ElementAtOrDefault(11) ?? "",

                Remarks = p.ElementAtOrDefault(14) ?? ""
            };
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
