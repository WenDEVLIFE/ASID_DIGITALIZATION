using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.SQLite
{
    public class SqliteLaneManagementRepository : ILaneManagementRepository
    {
        public IReadOnlyList<LaneManagement> GetAll()
        {
            using var connection = SqliteDatabase.CreateConnection();
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
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            var row = connection.QueryFirstOrDefault(@"
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
            using var connection = SqliteDatabase.CreateConnection();
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
            using var connection = SqliteDatabase.CreateConnection();
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
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            connection.Execute("DELETE FROM lane_management WHERE id = @Id;", new { Id = id });
        }

        public void SeedDefaultLanes()
        {
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            var existing = connection.Query<int>("SELECT COUNT(*) FROM lane_management;").FirstOrDefault();
            if (existing > 0)
            {
                // Check if all lanes are empty (Not Assigned, 0 stored)
                var hasAnyData = connection.Query<int>(
                    "SELECT COUNT(*) FROM lane_management WHERE actual_stored_qty > 0 OR part_no != 'Not Assigned';")
                    .FirstOrDefault();
                if (hasAnyData > 0) return;

                // All lanes empty — populate demo data
                SeedDemoData(connection);
                return;
            }

            var now = DateTime.UtcNow;
            var lanes = new List<LaneManagement>();

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

        public void IncrementStoredQty(string laneNo, string partNo, int quantity = 1)
        {
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            var existing = connection.QueryFirstOrDefault(
                "SELECT * FROM lane_management WHERE lane_no = @LaneNo LIMIT 1;",
                new { LaneNo = laneNo });

            if (existing == null)
            {
                Add(new LaneManagement
                {
                    LaneNo = laneNo,
                    PartNo = partNo,
                    MaxQtyStored = 100,
                    ActualStoredQty = quantity,
                    WithdrawnQty = 0,
                    LaneStatus = "Occupied",
                    ColorStatus = "Green",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                connection.Execute(@"
UPDATE lane_management SET
    actual_stored_qty = actual_stored_qty + @Qty,
    part_no = CASE WHEN part_no = 'Not Assigned' THEN @PartNo ELSE part_no END,
    updated_at = @Now
WHERE lane_no = @LaneNo;",
                    new { Qty = quantity, PartNo = partNo, Now = DateTime.UtcNow, LaneNo = laneNo });
            }

            RecalculateStatus(laneNo);
        }

        public void IncrementWithdrawnQty(string laneNo, int quantity = 1)
        {
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            connection.Execute(@"
UPDATE lane_management SET
    withdrawn_qty = withdrawn_qty + @Qty,
    updated_at = @Now
WHERE lane_no = @LaneNo;",
                new { Qty = quantity, Now = DateTime.UtcNow, LaneNo = laneNo });

            RecalculateStatus(laneNo);
        }

        public void RecalculateStatus(string laneNo)
        {
            using var connection = SqliteDatabase.CreateConnection();
            connection.Open();

            var lane = connection.QueryFirstOrDefault(
                "SELECT * FROM lane_management WHERE lane_no = @LaneNo LIMIT 1;",
                new { LaneNo = laneNo });

            if (lane == null) return;

            int stored = (int)lane.actual_stored_qty;
            int maxQty = (int)lane.max_qty_stored;
            string partNo = (string)lane.part_no;

            string status, color;

            if (stored >= maxQty)
            {
                status = "Full";
                color = "Red";
            }
            else if (stored > 0 && partNo != "Not Assigned")
            {
                status = "Occupied";
                color = "Green";
            }
            else if (stored == 0 && partNo != "Not Assigned")
            {
                status = "Vacant";
                color = "Green";
            }
            else
            {
                status = "Not Assigned";
                color = "Gray";
            }

            connection.Execute(@"
UPDATE lane_management SET
    lane_status = @Status,
    color_status = @Color,
    updated_at = @Now
WHERE lane_no = @LaneNo;",
                new { Status = status, Color = color, Now = DateTime.UtcNow, LaneNo = laneNo });
        }

        private void SeedDemoData(SqliteConnection connection)
        {
            var now = DateTime.UtcNow;

            // Demo lanes with test quantities to show movement
            var demos = new List<(string lane, string part, int max, int stored, int withdrawn)>
            {
                ("A-01", "657040000G", 10, 8, 3),
                ("A-02", "647187100F", 10, 10, 2),
                ("A-03", "657040000G", 10, 5, 5),
                ("A-04", "640578600E", 10, 3, 0),
                ("B-01", "647187000A", 10, 6, 1),
                ("B-02", "657040000G", 10, 0, 0),
                ("B-03", "640578600E", 10, 10, 10),
                ("B-04", "650436100H", 10, 2, 0),
                ("B-05", "647187100F", 10, 0, 3),
            };

            foreach (var d in demos)
            {
                string status = d.stored >= d.max ? "Full"
                    : d.stored > 0 ? "Occupied"
                    : "Vacant";
                string color = status == "Full" ? "Red" : "Green";

                connection.Execute(@"
UPDATE lane_management SET
    part_no = @PartNo,
    max_qty_stored = @MaxQty,
    actual_stored_qty = @Stored,
    withdrawn_qty = @Withdrawn,
    lane_status = @Status,
    color_status = @Color,
    updated_at = @Now
WHERE lane_no = @LaneNo;",
                    new
                    {
                        PartNo = d.part,
                        MaxQty = d.max,
                        Stored = d.stored,
                        Withdrawn = d.withdrawn,
                        Status = status,
                        Color = color,
                        Now = now,
                        LaneNo = d.lane
                    });
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
