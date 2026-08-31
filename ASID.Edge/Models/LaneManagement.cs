using System;

namespace ASID.Edge.Models
{
    public class LaneManagement
    {
        public int Id { get; set; }
        public string LaneNo { get; set; } = string.Empty;
        public string PartNo { get; set; } = string.Empty;
        public int MaxQtyStored { get; set; } = 100;
        public int ActualStoredQty { get; set; }
        public int WithdrawnQty { get; set; }
        public int OutstandingQty => ActualStoredQty - WithdrawnQty;
        public string LaneStatus { get; set; } = "Not Assigned"; // Not Assigned, Occupied, Full
        public string ColorStatus { get; set; } = "Gray"; // Green, Red, Gray
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
