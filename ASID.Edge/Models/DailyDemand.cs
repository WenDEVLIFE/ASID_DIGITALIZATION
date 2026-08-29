using System;
using System.Collections.Generic;
using System.Text;

namespace ASID.Edge.Models;

public class DailyDemand
{
    public long Id { get; set; }

    public DateTime ProductionDate { get; set; }

    public short Shift { get; set; }

    public string Model { get; set; } = "";

    public string PartNo { get; set; } = "";

    public int Quantity { get; set; }

    public int Scrapped { get; set; }

    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
