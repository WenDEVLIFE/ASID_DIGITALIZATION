using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class DailyDemandSummary
    {
        public string Model { get; set; } = "";

        public string PartNo { get; set; } = "";

        public int TotalDemand { get; set; }
    }
}
