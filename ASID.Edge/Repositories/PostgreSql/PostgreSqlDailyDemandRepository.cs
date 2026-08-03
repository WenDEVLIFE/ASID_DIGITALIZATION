using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;

namespace ASID.Edge.Repositories.PostgreSql;

public class PostgreSqlDailyDemandRepository
    : IDailyDemandRepository
{
    public void DeleteAll()
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        connection.Execute("DELETE FROM daily_demand;");
    }

    public void Insert(IEnumerable<DailyDemand> demands)
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        const string sql = @"
INSERT INTO daily_demand
(
    production_date,
    shift,
    model,
    part_no,
    quantity
)
VALUES
(
    @ProductionDate,
    @Shift,
    @Model,
    @PartNo,
    @Quantity
);";

        connection.Execute(sql, demands);
    }

    public List<DailyDemand> GetByDate(DateTime date)
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        const string sql = @"
SELECT
    id AS Id,
    CAST(production_date AS timestamp) AS ProductionDate,
    shift AS Shift,
    model AS Model,
    part_no AS PartNo,
    quantity AS Quantity
FROM daily_demand
WHERE production_date = @date
ORDER BY
    part_no,
    shift;";

        return connection.Query<DailyDemand>(
            sql,
            new { date })
            .ToList();
    }
}