using System;

namespace ASID.Edge.Models
{
    /// <summary>
    /// Lane management tracks trolley counts per lane.
    /// Each scan of a trolley into a lane = +1 to ActualStoredQty.
    /// SNP (items per trolley) is separate — stored in transactions table.
    /// </summary>
    public class LaneManagement
    {
        public int Id { get; set; }
        public string LaneNo { get; set; } = string.Empty;
        public string PartNo { get; set; } = string.Empty;
        /// <summary>Max number of trolleys this lane can hold</summary>
        public int MaxQtyStored { get; set; } = 100;
        /// <summary>Current trolley count (each scan = +1)</summary>
        public int ActualStoredQty { get; set; }
        /// <summary>Number of trolleys withdrawn from this lane</summary>
        public int WithdrawnQty { get; set; }
        /// <summary>Remaining trolleys = Stored - Withdrawn</summary>
        public int OutstandingQty => ActualStoredQty - WithdrawnQty;
        public string LaneStatus { get; set; } = "Not Assigned"; // Not Assigned, Vacant, Occupied, Full
        public string ColorStatus { get; set; } = "Gray"; // Green, Red, Gray
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
