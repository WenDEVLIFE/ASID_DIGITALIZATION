namespace ASID.Edge.Helpers
{
    /// <summary>
    /// Normalizes a PU-Body part number so values coming from different sources
    /// (scanned kanban/data-matrix vs the imported production plan) join reliably.
    /// The legacy KanbanParser stripped leading 'P' characters, so the same part
    /// can be stored with or without that prefix; comparison is therefore
    /// whitespace-trimmed, leading-'P'-tolerant and case-insensitive.
    /// </summary>
    public static class PartNoNormalizer
    {
        public static string Normalize(string? value) =>
            (value ?? string.Empty).Trim().TrimStart('P', 'p').ToUpperInvariant();
    }
}
