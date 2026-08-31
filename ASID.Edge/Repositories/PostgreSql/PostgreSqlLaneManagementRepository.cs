using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.PostgreSql
{
    public class PostgreSqlLaneManagementRepository : ILaneManagementRepository
    {
        public IReadOnlyList<LaneManagement> GetAll()
        {
            using var connection = Database.Database.CreateConnection();
            connection.Open();

            var rows = connection.Query(@"
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
ORDER BY lane_no;
").ToList();

            return rows.Select(r => new LaneManagement
            {
                Id = (int)r.Id,
                LaneNo = (string)r.LaneNo,
                PartNo = (string)r.PartNo,
                MaxQtyStored = (int)r.MaxQtyStored,
                ActualStoredQty = (int)r.ActualStoredQty,
                WithdrawnQty = (int)r.WithdrawnQty,
                LaneStatus = (string)r.LaneStatus,
                ColorStatus = (string)r.ColorStatus,
                CreatedAt = ParseDateTime(r.CreatedAt),
                UpdatedAt = ParseDateTime(r.UpdatedAt)
            }).ToList();
        }

        public LaneManagement? GetByLaneNo(string laneNo)
        {
            using var connection = Database.Database.CreateConnection();
            connection.Open();

            var row = connection.QueryFirstOrDefault<dynamic>(@"
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
WHERE lane_no = @LaneNo
LIMIT 1;", new { LaneNo = laneNo });

            if (row == null) return null;

            return new LaneManagement
            {
                Id = (int)row.Id,
                LaneNo = (string)row.LaneNo,
                PartNo = (string)row.PartNo,
                MaxQtyStored = (int)row.MaxQtyStored,
                ActualStoredQty = (int)row.ActualStoredQty,
                WithdrawnQty = (int)row.WithdrawnQty,
                LaneStatus = (string)row.LaneStatus,
                ColorStatus = (string)row.ColorStatus,
                CreatedAt = ParseDateTime(row.CreatedAt),
                UpdatedAt = ParseDateTime(row.UpdatedAt)
            };
        }

        public void Add(LaneManagement lane)
        {
            using var connection = Database.Database.CreateConnection();
            connection.Open();

            connection.Execute(@"
INSERT INTO lane_management
    (lane_no, part_no, max_qty_stored, actual_stored_qty, withdrawn_qty, lane_status, color_status, created_at, updated_at)
VALUES
    (@LaneNo, @PartNo, @MaxQtyStored, @ActualStoredQty, @WithdrawnQty, @LaneStatus, @ColorStatus, @CreatedAt, @UpdatedAt);
", lane);
        }

        public void Update(LaneManagement lane)
        {
            using var connection = Database.Database.CreateConnection();
            connection.Open();

            lane.UpdatedAt = DateTime.UtcNow;

            connection.Execute(@"
UPDATE lane_management SET
    part_no = @PartNo,
    max_qty_stored = @MaxQtyStored,
    actual_stored_qty = @ActualStoredQty,
    withdrawn_qty = @WithdrawnQty,
    lane_status = @LaneStatus,
    color_status = @ColorStatus,
    updated_at = @UpdatedAt
WHERE id = @Id;
", lane);
        }

        public void Delete(int id)
        {
            using var connection = Database.Database.CreateConnection();
            connection.Open();

            connection.Execute("DELETE FROM lane_management WHERE id = @Id;", new { Id = id });
        }

        public void SeedDefaultLanes()
        {
            using var connection = Database.Database.CreateConnection();
            connection.Open();

            var existing = connection.Query<int>("SELECT COUNT(*) FROM lane_management;").FirstOrDefault();
            if (existing > 0) return;

            var now = DateTime.UtcNow;
            var lanes = new List<LaneManagement>();

            // A-01 to A-50
            for (int i = 1; i <= 50; i++)
            {
                lanes.Add(new LaneManagement
                {
                    LaneNo = $"A-{i:D2}",
                    PartNo = "Not Assigned",
                    MaxQtyStored = 100,
                    ActualStoredQty = 0,
                    WithdrawnQty = 0,
                    LaneStatus = "Not Assigned",
                    ColorStatus = "Gray",
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            // B-01 to B-50
            for (int i = 1; i <= 50; i++)
            {
                lanes.Add(new LaneManagement
                {
                    LaneNo = $"B-{i:D2}",
                    PartNo = "Not Assigned",
                    MaxQtyStored = 100,
                    ActualStoredQty = 0,
                    WithdrawnQty = 0,
                    LaneStatus = "Not Assigned",
                    ColorStatus = "Gray",
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            foreach (var lane in lanes)
            {
                Add(lane);
            }
        }

        private static DateTime ParseDateTime(object? value)
        {
            if (value == null) return DateTime.MinValue;
            if (value is DateTime dt) return dt;
            if (DateTime.TryParse(value?.ToString(), out var parsed)) return parsed;
            return DateTime.MinValue;
        }
    }
}
