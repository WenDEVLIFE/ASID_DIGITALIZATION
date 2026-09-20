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
    /// Replaces demand only for the workweeks present in the file, then inserts the
    /// new records. Weeks already stored from other files are preserved, so plans
    /// accumulate across weeks.
    /// Returns the parse result so the caller can display the workweek label.
    /// </summary>
    public ExcelImporter.ParseResult ImportExcel(string filePath)
    {
        var result = ExcelImporter.Parse(filePath);

        // Per-week replacement: delete only the weeks parsed from this file, then
        // insert the parsed rows. Other weeks in daily_demand stay untouched.
        //
        // NOTE: this is NOT atomic. If an Insert fails after DeleteByWorkweek
        // succeeds, the affected week(s) are left empty. Re-importing the same file
        // is the idempotent recovery path: the week is deleted again and re-inserted,
        // so a failed import never duplicates rows.
        foreach (var weekStart in result.WeekStarts)
            _repository.DeleteByWorkweek(weekStart);

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
