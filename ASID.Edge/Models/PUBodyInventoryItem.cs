using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class PUBodyInventoryItem
    {
        public string Model { get; set; } = "";

        public string PartNo { get; set; } = "";

        public int InventoryByLocation_PUBodySupermarket { get; set; }

        public int InventoryFloating { get; set; }

        public int InventoryByLocation_P2LoadingBay { get; set; }

        public int InventoryByLocation_P1LoadingBay { get; set; }

        public int InventoryByLocation_P1Production { get; set; }
    }
}
