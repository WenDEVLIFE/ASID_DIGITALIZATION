namespace ASID.Edge.Models;

/// <summary>
/// Lane occupancy summary returned by the transactions repository.
/// A lane is considered occupied when <see cref="OpenCount"/> is greater
/// than zero (at least one transaction with a null consumed_at).
/// </summary>
public class LaneOccupancy
{
    public string LaneNo { get; set; } = "";

    public int OpenCount { get; set; }
}