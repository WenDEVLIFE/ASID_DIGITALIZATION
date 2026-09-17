using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace ASID.Edge.Models
{
    public class PUBodyInventoryItem
    {
        public string Model { get; set; } = "";

        public string PartNo { get; set; } = "";

        public int InventoryP2Supermarket { get; set; }

        public int InventoryFloating { get; set; }

        public int InventoryP2LoadingBay { get; set; }

        public int InventoryP1LoadingBay { get; set; }

        public int InventoryP1Production { get; set; }

        public Brush? P2SupermarketBackground { get; set; }
    }
}
