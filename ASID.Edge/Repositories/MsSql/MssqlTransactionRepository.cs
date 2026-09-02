using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ASID.Edge.Repositories.MsSql;

public class MssqlTransactionRepository : ITransactionRepository
{
    private static SqlConnection CreateConn()
    {
        return Database.Database.CreateMssqlConnection();
    }

    public void Add(StorageTransaction transaction)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
INSERT INTO transactions
(
    data_matrix, serial_no, model, part_no, quantity, kanban_no,
    operator_id, line_no, lane_no, trolley_no,
    station, status, created_at, updated_at,
    is_suspected_nc, is_nc_confirmed, is_nc_rejected, nc_quantity
)
VALUES
(
    @DataMatrix, @SerialNo, @Model, @PartNo, @SNP, @KanbanNo,
    @OperatorId, @LineNo, @LaneNo, @TrolleyNo,
    @Station, @Status, @CreatedAt, @UpdatedAt,
    @IsSuspectedNC, @IsNCConfirmed, @IsNCRejected, @NCQuantity
);";

        connection.Execute(sql, new
        {
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
            transaction.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            transaction.IsSuspectedNC,
            transaction.IsNCConfirmed,
            transaction.IsNCRejected,
            transaction.NCQuantity
        });
    }

    public StorageTransaction? GetByDataMatrix(string dataMatrix)
    {
        using var connection = CreateConn();
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
    status AS StatusText,
    is_suspected_nc AS IsSuspectedNC,
    is_nc_confirmed AS IsNCConfirmed,
    is_nc_rejected AS IsNCRejected,
    nc_quantity AS NCQuantity,
    created_at AS CreatedAt,
    updated_at AS UpdatedAt,
    withdrawn_at AS WithdrawnAt,
    forpickup_at AS ForPickupAt,
    received_at AS ReceivedAt,
    consumed_at AS ConsumedAt
FROM transactions
WHERE data_matrix = @dataMatrix;";

        var row = connection.QueryFirstOrDefault(sql, new { dataMatrix });
        if (row == null) return null;

        return MapRow(row);
    }

    public IReadOnlyList<StorageTransaction> GetAll()
    {
        using var connection = CreateConn();
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
    status AS StatusText,
    is_suspected_nc AS IsSuspectedNC,
    is_nc_confirmed AS IsNCConfirmed,
    is_nc_rejected AS IsNCRejected,
    nc_quantity AS NCQuantity,
    created_at AS CreatedAt,
    updated_at AS UpdatedAt,
    withdrawn_at AS WithdrawnAt,
    forpickup_at AS ForPickupAt,
    received_at AS ReceivedAt,
    consumed_at AS ConsumedAt
FROM transactions;";

        var rows = connection.Query(sql).ToList();
        var result = new List<StorageTransaction>();

        foreach (var row in rows)
        {
            result.Add(MapRow(row));
        }

        return result;
    }

    public IReadOnlyList<LaneOccupancy> GetLaneOccupancy()
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    lane_no AS LaneNo,
    COUNT(*) AS OpenCount
FROM transactions
WHERE lane_no IS NOT NULL AND lane_no != ''
  AND consumed_at IS NULL
GROUP BY lane_no
ORDER BY lane_no;";

        return connection.Query<LaneOccupancy>(sql).ToList();
    }

    public void Update(StorageTransaction transaction)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
UPDATE transactions
SET
    status = @Status,
    station = @Station,
    is_suspected_nc = @IsSuspectedNC,
    is_nc_confirmed = @IsNCConfirmed,
    is_nc_rejected = @IsNCRejected,
    nc_quantity = @NCQuantity,
    updated_at = @UpdatedAt,
    withdrawn_at = @WithdrawnAt,
    forpickup_at = @ForPickupAt,
    received_at = @ReceivedAt,
    consumed_at = @ConsumedAt
WHERE
    data_matrix = @DataMatrix;";

        connection.Execute(sql, new
        {
            Status = transaction.Status.ToString(),
            transaction.Station,
            transaction.IsSuspectedNC,
            transaction.IsNCConfirmed,
            transaction.IsNCRejected,
            transaction.NCQuantity,
            UpdatedAt = DateTime.UtcNow,
            transaction.WithdrawnAt,
            transaction.ForPickupAt,
            transaction.ReceivedAt,
            transaction.ConsumedAt,
            transaction.DataMatrix
        });
    }

    public bool DeleteByDataMatrix(string dataMatrix)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = "DELETE FROM transactions WHERE data_matrix = @dm;";
        int rows = connection.Execute(sql, new { dm = dataMatrix });
        return rows > 0;
    }

    private static StorageTransaction MapRow(dynamic row)
    {
        var dict = (IDictionary<string, object?>)row;

        string statusText = dict["StatusText"]?.ToString() ?? "Stored";
        MaterialStatus status = statusText.ToLowerInvariant() switch
        {
            "stored" => MaterialStatus.Stored,
            "withdrawn" => MaterialStatus.Withdrawn,
            "forpickup" => MaterialStatus.ForPickup,
            "received" => MaterialStatus.Received,
            "consumed" => MaterialStatus.Consumed,
            "scrapped" => MaterialStatus.Scrapped,
            _ => MaterialStatus.Stored
        };

        return new StorageTransaction
        {
            DataMatrix = dict["DataMatrix"]?.ToString() ?? "",
            SerialNo = dict["SerialNo"]?.ToString() ?? "",
            Model = dict["Model"]?.ToString() ?? "",
            PartNo = dict["PartNo"]?.ToString() ?? "",
            SNP = dict["SNP"] != null ? Convert.ToInt32(dict["SNP"]) : 0,
            KanbanNo = dict["KanbanNo"]?.ToString() ?? "",
            OperatorId = dict["OperatorId"]?.ToString() ?? "",
            LineNo = dict["LineNo"]?.ToString() ?? "",
            LaneNo = dict["LaneNo"]?.ToString() ?? "",
            TrolleyNo = dict["TrolleyNo"]?.ToString() ?? "",
            Station = dict["Station"]?.ToString() ?? "",
            Status = status,
            IsSuspectedNC = dict["IsSuspectedNC"] != null && Convert.ToBoolean(dict["IsSuspectedNC"]),
            IsNCConfirmed = dict["IsNCConfirmed"] != null && Convert.ToBoolean(dict["IsNCConfirmed"]),
            IsNCRejected = dict["IsNCRejected"] != null && Convert.ToBoolean(dict["IsNCRejected"]),
            NCQuantity = dict["NCQuantity"] != null ? Convert.ToInt32(dict["NCQuantity"]) : 0,
            CreatedAt = dict["CreatedAt"] != null ? Convert.ToDateTime(dict["CreatedAt"]) : DateTime.MinValue,
            WithdrawnAt = dict["WithdrawnAt"] != null ? Convert.ToDateTime(dict["WithdrawnAt"]) : null,
            ForPickupAt = dict["ForPickupAt"] != null ? Convert.ToDateTime(dict["ForPickupAt"]) : null,
            ReceivedAt = dict["ReceivedAt"] != null ? Convert.ToDateTime(dict["ReceivedAt"]) : null,
            ConsumedAt = dict["ConsumedAt"] != null ? Convert.ToDateTime(dict["ConsumedAt"]) : null
        };
    }
}
