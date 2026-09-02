using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.MsSql;

public class MssqlDailyDemandRepository : IDailyDemandRepository
{
    private static SqlConnection CreateConn()
    {
        return Database.Database.CreateMssqlConnection();
    }

    public void DeleteAll()
    {
        using var connection = CreateConn();
        connection.Open();
        connection.Execute("DELETE FROM daily_demand;");
    }

    public void DeleteByWorkweek(DateTime weekStart)
    {
        using var connection = CreateConn();
        connection.Open();
        connection.Execute(
            "DELETE FROM daily_demand WHERE production_date = @weekStart;",
            new { weekStart });
    }

    public void Insert(IEnumerable<DailyDemand> demands)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
INSERT INTO daily_demand
(production_date, shift, model, part_no, quantity, scrapped, imported_at)
VALUES
(@ProductionDate, @Shift, @Model, @PartNo, @Quantity, @Scrapped, @ImportedAt);";

        connection.Execute(sql, demands);
    }

    public List<DailyDemand> GetByDate(DateTime date)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    id AS Id,
    production_date AS ProductionDate,
    shift AS Shift,
    model AS Model,
    part_no AS PartNo,
    quantity AS Quantity,
    scrapped AS Scrapped,
    imported_at AS ImportedAt
FROM daily_demand
WHERE production_date = @date
ORDER BY part_no, shift;";

        return connection.Query<DailyDemand>(sql, new { date }).ToList();
    }

    public List<DailyDemand> GetAll()
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    id AS Id,
    production_date AS ProductionDate,
    shift AS Shift,
    model AS Model,
    part_no AS PartNo,
    quantity AS Quantity,
    scrapped AS Scrapped,
    imported_at AS ImportedAt
FROM daily_demand
ORDER BY production_date, part_no, shift;";

        return connection.Query<DailyDemand>(sql).ToList();
    }

    public DateTime? GetLastImportedAt()
    {
        try
        {
            using var connection = CreateConn();
            connection.Open();

            return connection.QuerySingleOrDefault<DateTime?>(
                "SELECT MAX(imported_at) FROM daily_demand;");
        }
        catch
        {
            return null;
        }
    }
}
