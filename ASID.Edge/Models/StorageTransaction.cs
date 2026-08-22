using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class StorageTransaction
    {
        public string Station { get; set; } = "";
        public string DataMatrix { get; set; } = "";

        public string OperatorId { get; set; } = "";

        public string KanbanNo { get; set; } = "";

        public string Model { get; set; } = "";

        public string PartNo { get; set; } = "";

        public int SNP { get; set; } 

        public string SerialNo { get; set; } = "";

        public string LineNo { get; set; } = "";

        public string TrolleyNo { get; set; } = "";

        public string LaneNo { get; set; } = "";

        public MaterialStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? WithdrawnAt { get; set; }

        public DateTime? ForPickupAt { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public DateTime? ConsumedAt { get; set; }
        public bool IsSuspectedNC { get; set; }
        public bool IsNCConfirmed { get; set; }
        public bool IsNCRejected { get; set; }

    }
}
