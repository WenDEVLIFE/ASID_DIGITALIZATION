using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class StorageTransaction
    {
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
    }
}
