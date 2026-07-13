using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class LabelData
    {
        public string DataMatrix { get; set; } = "";

        public string PartNo { get; set; } = "";

        public string Model { get; set; } = "";

        public string KanbanNo { get; set; } = "";

        public int Quantity { get; set; }

        public string LineNo { get; set; } = "";

        public string LaneNo { get; set; } = "";

        public string TrolleyNo { get; set; } = "";
    }
}
