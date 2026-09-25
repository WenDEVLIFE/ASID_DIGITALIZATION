using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.PostgreSql;

public class PostgreSqlPlantReturnRepository
    : IPlantReturnRepository
{
    public void Add(PlantReturn plantReturn)
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        const string sql = @"
INSERT INTO plant_return
(id, part_no, quantity, remarks, username, created_at)
VALUES
(@Id, @PartNo, @Quantity, @Remarks, @Username, @CreatedAt);";

        connection.Execute(sql, new
        {
            Id = plantReturn.Id == Guid.Empty ? Guid.NewGuid() : plantReturn.Id,
            plantReturn.PartNo,
            plantReturn.Quantity,
            plantReturn.Remarks,
            plantReturn.Username,
            CreatedAt = plantReturn.CreatedAt == default(DateTime)
                ? DateTime.UtcNow
                : plantReturn.CreatedAt
        });
    }

    public IReadOnlyList<PlantReturn> GetAll()
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        const string sql = @"
SELECT
    id AS Id,
    part_no AS PartNo,
    quantity AS Quantity,
    remarks AS Remarks,
    username AS Username,
    created_at AS CreatedAt
FROM plant_return
ORDER BY created_at;";

        return connection.Query<PlantReturn>(sql).ToList();
    }

    public IReadOnlyList<PlantReturn> GetByWeek(DateTime weekStart)
    {
        using var connection =
            Database.Database.CreateConnection();

        connection.Open();

        const string sql = @"
SELECT
    id AS Id,
    part_no AS PartNo,
    quantity AS Quantity,
    remarks AS Remarks,
    username AS Username,
    created_at AS CreatedAt
FROM plant_return
WHERE created_at >= @weekStart
  AND created_at < @weekStart + INTERVAL '7 day'
ORDER BY created_at;";

        return connection.Query<PlantReturn>(sql, new { weekStart }).ToList();
    }
}
