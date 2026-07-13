using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models
{
    public class PUBodyWithdrawalItem
    {
        public string Date { get; set; } = "";

        public string Time { get; set; } = "";

        public string Model { get; set; } = "";

        public string PartNo { get; set; } = "";

        public string TrolleyNo { get; set; } = "";

        public int Quantity { get; set; }
    }
}
