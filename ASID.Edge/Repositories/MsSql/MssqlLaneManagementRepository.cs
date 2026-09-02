using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.MsSql;

public class MssqlLaneManagementRepository : ILaneManagementRepository
{
    private static SqlConnection CreateConn()
    {
        return Database.Database.CreateMssqlConnection();
    }

    public IReadOnlyList<LaneManagement> GetAll()
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    id AS Id,
    lane_no AS LaneNo,
    part_no AS PartNo,
    max_qty_stored AS MaxQtyStored,
    actual_stored_qty AS ActualStoredQty,
    withdrawn_qty AS WithdrawnQty,
    lane_status AS LaneStatus,
    color_status AS ColorStatus,
    created_at AS CreatedAt,
    updated_at AS UpdatedAt
FROM lane_management
ORDER BY lane_no;";

        return connection.Query<LaneManagement>(sql).ToList();
    }

    public LaneManagement? GetByLaneNo(string laneNo)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    id AS Id,
    lane_no AS LaneNo,
    part_no AS PartNo,
    max_qty_stored AS MaxQtyStored,
    actual_stored_qty AS ActualStoredQty,
    withdrawn_qty AS WithdrawnQty,
    lane_status AS LaneStatus,
    color_status AS ColorStatus,
    created_at AS CreatedAt,
    updated_at AS UpdatedAt
FROM lane_management
WHERE lane_no = @laneNo;";

        return connection.QueryFirstOrDefault<LaneManagement>(sql, new { laneNo });
    }

    public void Add(LaneManagement lane)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
INSERT INTO lane_management (lane_no, part_no, max_qty_stored, actual_stored_qty, withdrawn_qty, lane_status, color_status, created_at, updated_at)
VALUES (@LaneNo, @PartNo, @MaxQtyStored, @ActualStoredQty, @WithdrawnQty, @LaneStatus, @ColorStatus, @CreatedAt, @UpdatedAt);";

        connection.Execute(sql, lane);
    }

    public void Update(LaneManagement lane)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
UPDATE lane_management
SET part_no = @PartNo,
    max_qty_stored = @MaxQtyStored,
    actual_stored_qty = @ActualStoredQty,
    withdrawn_qty = @WithdrawnQty,
    lane_status = @LaneStatus,
    color_status = @ColorStatus,
    updated_at = @UpdatedAt
WHERE id = @Id;";

        connection.Execute(sql, lane);
    }

    public void Delete(int id)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = "DELETE FROM lane_management WHERE id = @Id;";
        connection.Execute(sql, new { Id = id });
    }

    public void SeedDefaultLanes()
    {
        using var connection = CreateConn();
        connection.Open();

        // Check if lanes already exist
        int count = connection.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM lane_management;");
        if (count > 0) return;

        // Seed default lanes A1-A10, B1-B10
        var lanes = new List<string>();
        foreach (char row in new[] { 'A', 'B' })
        {
            for (int i = 1; i <= 10; i++)
            {
                lanes.Add($"{row}{i}");
            }
        }

        const string sql = @"
INSERT INTO lane_management (lane_no, part_no, max_qty_stored, actual_stored_qty, withdrawn_qty, lane_status, color_status, created_at, updated_at)
VALUES (@LaneNo, 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray', @Now, @Now);";

        foreach (var lane in lanes)
        {
            connection.Execute(sql, new { LaneNo = lane, Now = DateTime.UtcNow });
        }
    }

    public void IncrementStoredQty(string laneNo, string partNo, int quantity = 1)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
UPDATE lane_management
SET actual_stored_qty = actual_stored_qty + @Qty,
    part_no = @PartNo,
    updated_at = @Now
WHERE lane_no = @LaneNo;";

        connection.Execute(sql, new { Qty = quantity, PartNo = partNo, LaneNo = laneNo, Now = DateTime.UtcNow });
    }

    public void IncrementWithdrawnQty(string laneNo, int quantity = 1)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
UPDATE lane_management
SET withdrawn_qty = withdrawn_qty + @Qty,
    updated_at = @Now
WHERE lane_no = @LaneNo;";

        connection.Execute(sql, new { Qty = quantity, LaneNo = laneNo, Now = DateTime.UtcNow });
    }

    public void RecalculateStatus(string laneNo)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
UPDATE lane_management
SET lane_status = @Status,
    color_status = @Color,
    updated_at = @Now
WHERE lane_no = @LaneNo;";

        // Get current counts
        var lane = GetByLaneNo(laneNo);
        if (lane == null) return;

        int balance = lane.ActualStoredQty - lane.WithdrawnQty;
        string status, color;

        if (lane.PartNo == "Not Assigned" || lane.PartNo == "")
        {
            status = "Not Assigned";
            color = "Gray";
        }
        else if (balance <= 0)
        {
            status = "Vacant";
            color = "Green";
        }
        else if (balance >= lane.MaxQtyStored)
        {
            status = "Full";
            color = "Red";
        }
        else
        {
            status = "Occupied";
            color = "Green";
        }

        connection.Execute(sql, new { Status = status, Color = color, LaneNo = laneNo, Now = DateTime.UtcNow });
    }
}
