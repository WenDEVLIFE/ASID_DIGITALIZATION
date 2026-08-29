using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.PostgreSql;

public class PostgreSqlDailyDemandRepository
    : IDailyDemandRepository
{
    // Cached flag: true once we confirm the new columns exist.
    private static bool? _hasNewColumns;

    private static bool HasNewColumns()
    {
        if (_hasNewColumns.HasValue)
            return _hasNewColumns.Value;

        try
        {
            using var conn = Database.Database.CreateConnection();
            conn.Open();
            // If this query succeeds, the columns exist.
            conn.Execute(
                "SELECT scrapped, imported_at FROM daily_demand LIMIT 0;");
            _hasNewColumns = true;
        }
        catch
        {
            _hasNewColumns = false;
        }

        return _hasNewColumns.Value;
    }

    public void DeleteAll()
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        connection.Execute("DELETE FROM daily_demand;");
    }

    public void DeleteByWorkweek(DateTime weekStart)
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        connection.Execute(
            "DELETE FROM daily_demand WHERE production_date = @weekStart;",
            new { weekStart });
    }

    public void Insert(IEnumerable<DailyDemand> demands)
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        if (HasNewColumns())
        {
            const string sql = @"
INSERT INTO daily_demand
(
    production_date, shift, model, part_no,
    quantity, scrapped, imported_at
)
VALUES
(
    @ProductionDate, @Shift, @Model, @PartNo,
    @Quantity, @Scrapped, @ImportedAt
);";
            connection.Execute(sql, demands);
        }
        else
        {
            // Fallback: old schema without scrapped / imported_at
            const string sql = @"
INSERT INTO daily_demand
(
    production_date, shift, model, part_no, quantity
)
VALUES
(
    @ProductionDate, @Shift, @Model, @PartNo, @Quantity
);";
            connection.Execute(sql, demands);
        }
    }

    public List<DailyDemand> GetByDate(DateTime date)
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        if (HasNewColumns())
        {
            const string sql = @"
SELECT
    id AS Id,
    CAST(production_date AS timestamp) AS ProductionDate,
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
        else
        {
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
ORDER BY part_no, shift;";
            return connection.Query<DailyDemand>(sql, new { date }).ToList();
        }
    }

    public List<DailyDemand> GetAll()
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        if (HasNewColumns())
        {
            const string sql = @"
SELECT
    id AS Id,
    CAST(production_date AS timestamp) AS ProductionDate,
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
        else
        {
            const string sql = @"
SELECT
    id AS Id,
    CAST(production_date AS timestamp) AS ProductionDate,
    shift AS Shift,
    model AS Model,
    part_no AS PartNo,
    quantity AS Quantity
FROM daily_demand
ORDER BY production_date, part_no, shift;";
            return connection.Query<DailyDemand>(sql).ToList();
        }
    }

    public DateTime? GetLastImportedAt()
    {
        if (!HasNewColumns())
            return null;

        try
        {
            using var connection =
                Database.Database.CreateConnection();

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