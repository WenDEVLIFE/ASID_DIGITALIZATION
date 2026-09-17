using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class PUBodyDailyDemandItem
    {
        public string Date { get; set; } = "";

        public string Model { get; set; } = "";

        public string PartNo { get; set; } = "";

        public int Demand { get; set; }

        public int P2Inventory { get; set; }

        public int DeliveredToP1 { get; set; }

        public int Scrapped { get; set; }

        /// <summary>
        /// Variance = DeliveredToP1 (current ISO week) minus Demand plus Scrapped.
        /// </summary>
        public int Variance => DeliveredToP1 - Demand - Scrapped;
    }
}
