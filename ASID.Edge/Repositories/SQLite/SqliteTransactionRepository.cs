using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASID.Edge.Repositories.SQLite;

/// <summary>
/// Local-first transaction repository backed by SQLite.
/// Every scan writes here first (fast, works offline).
/// A background sync service later pushes un-synced rows to PostgreSQL.
/// </summary>
public class SqliteTransactionRepository : ITransactionRepository
{
    /// <summary>
    /// Raised after every Add or Update so the SyncService can push
    /// the new/changed row to PostgreSQL immediately.
    /// </summary>
    public event Action? TransactionChanged;

    public void Add(StorageTransaction transaction)
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        // Ensure the id is set (GUID if empty).
        if (string.IsNullOrEmpty(transaction.DataMatrix))
            return;

        // Generate a stable UUID-style id from the data matrix if not set.
        string id = Guid.NewGuid().ToString("D");

        const string sql = @"
INSERT OR IGNORE INTO transactions
(
    id,
    data_matrix,
    serial_no,
    model,
    part_no,
    quantity,
    kanban_no,
    operator_id,
    line_no,
    lane_no,
    trolley_no,
    station,
    status,
    created_at,
    updated_at,
    withdrawn_at,
    forpickup_at,
    received_at,
    consumed_at,
    is_suspected_nc,
    is_nc_confirmed,
    is_nc_rejected,
    nc_quantity,
    synced
)
VALUES
(
    @Id,
    @DataMatrix,
    @SerialNo,
    @Model,
    @PartNo,
    @SNP,
    @KanbanNo,
    @OperatorId,
    @LineNo,
    @LaneNo,
    @TrolleyNo,
    @Station,
    @Status,
    @CreatedAt,
    @UpdatedAt,
    @WithdrawnAt,
    @ForPickupAt,
    @ReceivedAt,
    @ConsumedAt,
    @IsSuspectedNC,
    @IsNCConfirmed,
    @IsNCRejected,
    @NCQuantity,
    0
);";

        connection.Execute(sql, new
        {
            Id = id,
            transaction.DataMatrix,
            transaction.SerialNo,
            transaction.Model,
            transaction.PartNo,
            transaction.SNP,
            transaction.KanbanNo,
            transaction.OperatorId,
            transaction.LineNo,
            transaction.LaneNo,
            transaction.TrolleyNo,
            transaction.Station,
            Status = transaction.Status.ToString(),
            CreatedAt = transaction.CreatedAt.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            WithdrawnAt = transaction.WithdrawnAt?.ToString("o"),
            ForPickupAt = transaction.ForPickupAt?.ToString("o"),
            ReceivedAt = transaction.ReceivedAt?.ToString("o"),
            ConsumedAt = transaction.ConsumedAt?.ToString("o"),
            IsSuspectedNC = transaction.IsSuspectedNC ? 1 : 0,
            IsNCConfirmed = transaction.IsNCConfirmed ? 1 : 0,
            IsNCRejected = transaction.IsNCRejected ? 1 : 0,
            transaction.NCQuantity
        });

        TransactionChanged?.Invoke();
    }

    public StorageTransaction? GetByDataMatrix(string dataMatrix)
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        const string sql = @"
SELECT
    data_matrix AS DataMatrix,
    serial_no AS SerialNo,
    model AS Model,
    part_no AS PartNo,
    quantity AS SNP,
    kanban_no AS KanbanNo,
    operator_id AS OperatorId,
    line_no AS LineNo,
    lane_no AS LaneNo,
    trolley_no AS TrolleyNo,
    station AS Station,
    status AS Status,
    is_suspected_nc AS IsSuspectedNC,
    is_nc_confirmed AS IsNCConfirmed,
    is_nc_rejected AS IsNCRejected,
    nc_quantity AS NCQuantity,
    created_at AS CreatedAt,
    withdrawn_at AS WithdrawnAt,
    forpickup_at AS ForPickupAt,
    received_at AS ReceivedAt,
    consumed_at AS ConsumedAt
FROM transactions
WHERE data_matrix = @dataMatrix
LIMIT 1;";

        var row = connection.QueryFirstOrDefault<dynamic>(
            sql,
            new { dataMatrix });

        return row == null ? null : MapRow(row);
    }

    public IReadOnlyList<StorageTransaction> GetAll()
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        var rows = connection.Query<dynamic>(@"
SELECT
    data_matrix AS DataMatrix,
    serial_no AS SerialNo,
    model AS Model,
    part_no AS PartNo,
    quantity AS SNP,
    kanban_no AS KanbanNo,
    operator_id AS OperatorId,
    line_no AS LineNo,
    lane_no AS LaneNo,
    trolley_no AS TrolleyNo,
    station AS Station,
    status AS Status,
    is_suspected_nc AS IsSuspectedNC,
    is_nc_confirmed AS IsNCConfirmed,
    is_nc_rejected AS IsNCRejected,
    nc_quantity AS NCQuantity,
    created_at AS CreatedAt,
    withdrawn_at AS WithdrawnAt,
    forpickup_at AS ForPickupAt,
    received_at AS ReceivedAt,
    consumed_at AS ConsumedAt
FROM transactions
ORDER BY created_at DESC;
").ToList();

        return rows.Select(MapRow).ToList();
    }

    public void Update(StorageTransaction transaction)
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        const string sql = @"
UPDATE transactions
SET
    status = @Status,
    is_suspected_nc = @IsSuspectedNC,
    is_nc_confirmed = @IsNCConfirmed,
    is_nc_rejected = @IsNCRejected,
    nc_quantity = @NCQuantity,
    station = @Station,
    withdrawn_at = @WithdrawnAt,
    forpickup_at = @ForPickupAt,
    received_at = @ReceivedAt,
    consumed_at = @ConsumedAt,
    updated_at = @UpdatedAt,
    synced = 0
WHERE
    data_matrix = @DataMatrix;";

        connection.Execute(sql, new
        {
            transaction.Status,
            IsSuspectedNC = transaction.IsSuspectedNC ? 1 : 0,
            IsNCConfirmed = transaction.IsNCConfirmed ? 1 : 0,
            IsNCRejected = transaction.IsNCRejected ? 1 : 0,
            transaction.NCQuantity,
            transaction.Station,
            WithdrawnAt = transaction.WithdrawnAt?.ToString("o"),
            ForPickupAt = transaction.ForPickupAt?.ToString("o"),
            ReceivedAt = transaction.ReceivedAt?.ToString("o"),
            ConsumedAt = transaction.ConsumedAt?.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o"),
            transaction.DataMatrix
        });

        TransactionChanged?.Invoke();
    }

    public IReadOnlyList<LaneOccupancy> GetLaneOccupancy()
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        const string sql = @"
SELECT
    lane_no AS LaneNo,
    COUNT(*) AS OpenCount
FROM transactions
WHERE lane_no IS NOT NULL AND lane_no != ''
  AND (consumed_at IS NULL OR consumed_at = '')
GROUP BY lane_no
ORDER BY lane_no;";

        return connection.Query<LaneOccupancy>(sql).ToList();
    }

    public bool DeleteByDataMatrix(string dataMatrix)
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        const string sql = "DELETE FROM transactions WHERE data_matrix = @dm;";
        int rows = connection.Execute(sql, new { dm = dataMatrix });

        if (rows > 0)
            TransactionChanged?.Invoke();

        return rows > 0;
    }

    // ── Helpers ──

    private static DateTime ParseDateTime(object? value)
    {
        if (value == null)
            return DateTime.MinValue;

        if (value is DateTime dt)
            return dt;

        string? s = value?.ToString();
        if (string.IsNullOrWhiteSpace(s))
            return DateTime.MinValue;

        if (DateTime.TryParse(s, out DateTime parsed))
            return parsed;

        return DateTime.MinValue;
    }

    private static StorageTransaction MapRow(dynamic row)
    {
        return new StorageTransaction
        {
            DataMatrix = (string)row.DataMatrix,
            SerialNo = (string)row.SerialNo,
            Model = (string)row.Model,
            PartNo = (string)row.PartNo,
            SNP = (int)row.SNP,
            KanbanNo = (string)row.KanbanNo,
            OperatorId = (string)row.OperatorId,
            LineNo = (string)row.LineNo,
            LaneNo = (string)row.LaneNo,
            TrolleyNo = (string)row.TrolleyNo,
            Station = (string)row.Station,
            Status = Enum.TryParse<MaterialStatus>((string)row.Status, out var s) ? s : MaterialStatus.Stored,
            IsSuspectedNC = ((int)row.IsSuspectedNC) == 1,
            IsNCConfirmed = ((int)row.IsNCConfirmed) == 1,
            IsNCRejected = ((int)row.IsNCRejected) == 1,
            NCQuantity = (int)row.NCQuantity,
            CreatedAt = ParseDateTime(row.CreatedAt),
            WithdrawnAt = ParseDateTime(row.WithdrawnAt),
            ForPickupAt = ParseDateTime(row.ForPickupAt),
            ReceivedAt = ParseDateTime(row.ReceivedAt),
            ConsumedAt = ParseDateTime(row.ConsumedAt)
        };
    }

    // ── Sync helpers (used by SyncService) ──

    /// <summary>
    /// Get all transactions that have not yet been pushed to PostgreSQL.
    /// </summary>
    public IReadOnlyList<StorageTransaction> GetUnsynced()
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        return connection.Query<StorageTransaction>(@"
SELECT
    data_matrix AS DataMatrix,
    serial_no AS SerialNo,
    model AS Model,
    part_no AS PartNo,
    quantity AS SNP,
    kanban_no AS KanbanNo,
    operator_id AS OperatorId,
    line_no AS LineNo,
    lane_no AS LaneNo,
    trolley_no AS TrolleyNo,
    station AS Station,
    status AS Status,
    is_suspected_nc AS IsSuspectedNC,
    is_nc_confirmed AS IsNCConfirmed,
    is_nc_rejected AS IsNCRejected,
    nc_quantity AS NCQuantity,
    created_at AS CreatedAt,
    withdrawn_at AS WithdrawnAt,
    forpickup_at AS ForPickupAt,
    received_at AS ReceivedAt,
    consumed_at AS ConsumedAt
FROM transactions
WHERE synced = 0;
").ToList();
    }

    /// <summary>
    /// Mark a transaction as synced after successful push to PostgreSQL.
    /// </summary>
    public void MarkSynced(string dataMatrix)
    {
        using var connection = SqliteDatabase.CreateConnection();
        connection.Open();

        connection.Execute(
            "UPDATE transactions SET synced = 1 WHERE data_matrix = @dataMatrix;",
            new { dataMatrix });
    }
}
