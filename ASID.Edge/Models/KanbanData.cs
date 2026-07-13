namespace ASID.Edge.Models
{
    public class KanbanData
    {
        public string ProductionLoop { get; set; } = "";
        public string Model { get; set; } = "";

        public string PartNo { get; set; } = "";

        public int Quantity { get; set; }

        public string KanbanNo { get; set; } = "";

        public string Supplier { get; set; } = "";

        public string Customer { get; set; } = "";

        public string Branch { get; set; } = "";

        public string Remarks { get; set; } = "";
    }
}