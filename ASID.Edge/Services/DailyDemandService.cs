using ASID.Edge.Helpers;
using ASID.Edge.Repositories.Interfaces;

namespace ASID.Edge.Services;

public class DailyDemandService
{
    private readonly IDailyDemandRepository _repository;

    public DailyDemandService(IDailyDemandRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Parses the workbook and replaces the whole daily demand plan.
    /// Returns the number of imported rows.
    /// </summary>
    /// <remarks>
    /// Parsing happens BEFORE DeleteAll so a malformed file fails without
    /// touching the database. DeleteAll wipes demand for ALL dates, not just
    /// the dates in the imported file — accepted product decision.
    /// </remarks>
    public int ImportExcel(string filePath)
    {
        var demands = ExcelImporter.Parse(filePath);

        _repository.DeleteAll();

        _repository.Insert(demands);

        return demands.Count;
    }
}
