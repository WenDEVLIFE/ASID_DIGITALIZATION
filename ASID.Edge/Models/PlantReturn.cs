using System;

namespace ASID.Edge.Models;

public class PlantReturn
{
    public Guid Id { get; set; }

    public string PartNo { get; set; } = "";

    public int Quantity { get; set; }

    public string? Remarks { get; set; }

    public string? Username { get; set; }

    public DateTime CreatedAt { get; set; }
}
