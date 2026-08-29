using ASID.Edge.Helpers;
using ASID.Edge.Repositories.Interfaces;
using System;

namespace ASID.Edge.Services;

public class DailyDemandService
{
    private readonly IDailyDemandRepository _repository;

    public DailyDemandService(IDailyDemandRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Import a production-plan Excel file.
    /// Deletes existing demand for the same workweek, then inserts the new records.
    /// Returns the parse result so the caller can display the workweek label.
    /// </summary>
    public ExcelImporter.ParseResult ImportExcel(string filePath)
    {
        var result = ExcelImporter.Parse(filePath);

        // Delete all existing demand before importing (full workweek replacement)
        _repository.DeleteAll();

        _repository.Insert(result.Demands);

        return result;
    }

    /// <summary>
    /// Check if the demand data has been updated since the given timestamp.
    /// Used for change detection (e.g., polling or timer-based refresh).
    /// </summary>
    public bool HasDataChanged(DateTime? lastKnownImport)
    {
        var lastImportedAt = _repository.GetLastImportedAt();

        if (lastImportedAt == null)
            return false;

        if (lastKnownImport == null)
            return true;

        return lastImportedAt.Value > lastKnownImport.Value;
    }

    /// <summary>
    /// Get the timestamp of the most recent import.
    /// </summary>
    public DateTime? GetLastImportTimestamp()
    {
        return _repository.GetLastImportedAt();
    }
}
