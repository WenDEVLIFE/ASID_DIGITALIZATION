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
    barcode, model, part_no, lane, status, snp,
    is_nc_confirmed, nc_quantity, operator, station,
    created_at, updated_at
)
VALUES
(
    @DataMatrix, @Model, @PartNo, @LaneNo, @Status, @SNP,
    @IsNCConfirmed, @NCQuantity, @OperatorId, @Station,
    @CreatedAt, @UpdatedAt
);";

        connection.Execute(sql, new
        {
            transaction.DataMatrix,
            transaction.Model,
            transaction.PartNo,
            transaction.LaneNo,
            Status = (int)transaction.Status,
            transaction.SNP,
            transaction.IsNCConfirmed,
            transaction.NCQuantity,
            transaction.OperatorId,
            transaction.Station,
            transaction.CreatedAt,
            UpdatedAt = transaction.CreatedAt
        });
    }

    public StorageTransaction? GetByDataMatrix(string dataMatrix)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    barcode AS DataMatrix,
    model AS Model,
    part_no AS PartNo,
    lane AS LaneNo,
    status AS Status,
    snp AS SNP,
    is_nc_confirmed AS IsNCConfirmed,
    nc_quantity AS NCQuantity,
    operator AS OperatorId,
    station AS Station,
    created_at AS CreatedAt,
    created_at AS CreatedAt
FROM transactions
WHERE barcode = @dataMatrix;";

        var row = connection.QueryFirstOrDefault(sql, new { dataMatrix });
        if (row == null) return null;

        // Map dynamic to StorageTransaction
        var dict = (IDictionary<string, object?>)row;
        return new StorageTransaction
        {
            DataMatrix = dict["DataMatrix"]?.ToString() ?? "",
            Model = dict["Model"]?.ToString() ?? "",
            PartNo = dict["PartNo"]?.ToString() ?? "",
            LaneNo = dict["LaneNo"]?.ToString() ?? "",
            Status = dict["Status"] != null ? (MaterialStatus)(int)dict["Status"]! : MaterialStatus.Stored,
            SNP = dict["SNP"] != null ? (int)dict["SNP"]! : 0,
            IsNCConfirmed = dict["IsNCConfirmed"] != null && (bool)dict["IsNCConfirmed"]!,
            NCQuantity = dict["NCQuantity"] != null ? (int)dict["NCQuantity"]! : 0,
            OperatorId = dict["OperatorId"]?.ToString() ?? "",
            Station = dict["Station"]?.ToString() ?? "",
            CreatedAt = dict["CreatedAt"] != null ? (DateTime)dict["CreatedAt"]! : DateTime.MinValue
        };
    }

    public IReadOnlyList<StorageTransaction> GetAll()
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    barcode AS DataMatrix,
    model AS Model,
    part_no AS PartNo,
    lane AS LaneNo,
    status AS Status,
    snp AS SNP,
    is_nc_confirmed AS IsNCConfirmed,
    nc_quantity AS NCQuantity,
    operator AS OperatorId,
    station AS Station,
    created_at AS CreatedAt,
    created_at AS CreatedAt
FROM transactions;";

        var rows = connection.Query(sql).ToList();
        var result = new List<StorageTransaction>();

        foreach (var row in rows)
        {
            var dict = (IDictionary<string, object?>)row;
            result.Add(new StorageTransaction
            {
                DataMatrix = dict["DataMatrix"]?.ToString() ?? "",
                Model = dict["Model"]?.ToString() ?? "",
                PartNo = dict["PartNo"]?.ToString() ?? "",
                LaneNo = dict["LaneNo"]?.ToString() ?? "",
                Status = dict["Status"] != null ? (MaterialStatus)(int)dict["Status"]! : MaterialStatus.Stored,
                SNP = dict["SNP"] != null ? (int)dict["SNP"]! : 0,
                IsNCConfirmed = dict["IsNCConfirmed"] != null && (bool)dict["IsNCConfirmed"]!,
                NCQuantity = dict["NCQuantity"] != null ? (int)dict["NCQuantity"]! : 0,
                OperatorId = dict["OperatorId"]?.ToString() ?? "",
                Station = dict["Station"]?.ToString() ?? "",
                CreatedAt = dict["CreatedAt"] != null ? (DateTime)dict["CreatedAt"]! : DateTime.MinValue
            });
        }

        return result;
    }

    public IReadOnlyList<LaneOccupancy> GetLaneOccupancy()
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = @"
SELECT
    lane AS LaneNo,
    COUNT(*) AS OpenCount
FROM transactions
WHERE lane IS NOT NULL AND lane != ''
  AND station NOT IN ('Consumed', 'Scrapped')
GROUP BY lane
ORDER BY lane;";

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
    is_nc_confirmed = @IsNCConfirmed,
    nc_quantity = @NCQuantity,
    station = @Station,
    updated_at = @UpdatedAt
WHERE
    barcode = @DataMatrix;";

        connection.Execute(sql, new
        {
            Status = (int)transaction.Status,
            transaction.IsNCConfirmed,
            transaction.NCQuantity,
            transaction.Station,
            UpdatedAt = DateTime.UtcNow,
            transaction.DataMatrix
        });
    }

    public bool DeleteByDataMatrix(string dataMatrix)
    {
        using var connection = CreateConn();
        connection.Open();

        const string sql = "DELETE FROM transactions WHERE barcode = @dm;";
        int rows = connection.Execute(sql, new { dm = dataMatrix });
        return rows > 0;
    }
}
