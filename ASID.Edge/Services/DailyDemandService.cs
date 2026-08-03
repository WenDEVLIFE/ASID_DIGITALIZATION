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

    public void ImportExcel(string filePath)
    {
        var demands = ExcelImporter.Parse(filePath);

        _repository.DeleteAll();

        _repository.Insert(demands);
    }
}
