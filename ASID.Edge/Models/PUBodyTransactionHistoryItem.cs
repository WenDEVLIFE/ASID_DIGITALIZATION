using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class PUBodyTransactionHistoryItem
    {
        public MaterialStatus Status { get; set; }
        public string Model { get; set; } = "";

        public string KanbanNo { get; set; } = "";

        public string PartNo { get; set; } = "";

        public string SerialNo { get; set; } = "";

        public int SNP { get; set; }

        public string LineNo { get; set; } = "";

        public string TrolleyNo { get; set; } = "";

        public string LaneNo { get; set; } = "";

        public string Date { get; set; } = "";

        public string Time { get; set; } = "";

        public bool IsSuspectedNC { get; set; }
        public bool IsNCConfirmed { get; set; }
        public bool IsNCRejected { get; set; }
        public int NCQuantity { get; set; }

        public string NCIndicator =>
            IsNCConfirmed ? (NCQuantity > 0 ? $"NC (Qty: {NCQuantity})" : "NC") :
            IsNCRejected ? "Rejected NC" :
            IsSuspectedNC ? (NCQuantity > 0 ? $"Suspected NC ({NCQuantity})" : "Suspected NC") : "";

        public string NCRemarks => NCIndicator;
    }
}
