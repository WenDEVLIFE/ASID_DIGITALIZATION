using ASID.Edge.Database;
using ASID.Edge.Models;
using ASID.Edge.Repositories.Interfaces;
using Dapper;
using System.Windows;

namespace ASID.Edge.Repositories.PostgreSql
{
    public class PostgreSqlTransactionRepository
        : ITransactionRepository
    {
        public void Add(StorageTransaction transaction)
        {
            using var connection = Database.Database.CreateConnection();


            connection.Open();

            const string sql = @"
INSERT INTO transactions
(
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
    status
)
VALUES
(
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
    @Status
);";

            connection.Execute(sql, transaction);
        }

        public StorageTransaction? GetByDataMatrix(string dataMatrix)
        {
            using var connection =
                Database.Database.CreateConnection();

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
    created_at AS CreatedAt,
    withdrawn_at AS WithdrawnAt,
forpickup_at AS ForPickupAt,
received_at AS ReceivedAt,
consumed_at AS ConsumedAt
FROM transactions
WHERE data_matrix = @dataMatrix
LIMIT 1;";

            return connection.QueryFirstOrDefault<StorageTransaction>(
                sql,
                new { dataMatrix });
        }

        public IReadOnlyList<StorageTransaction> GetAll()
        {
            using var connection =
                Database.Database.CreateConnection();

            connection.Open();

            return connection.Query<StorageTransaction>(
            @"
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
    created_at AS CreatedAt,
    withdrawn_at AS WithdrawnAt,
forpickup_at AS ForPickupAt,
received_at AS ReceivedAt,
consumed_at AS ConsumedAt
FROM transactions;
")
            .ToList();
        }

        public void Update(StorageTransaction transaction)
        {
            using var connection =
                Database.Database.CreateConnection();

            connection.Open();

            const string sql = @"
UPDATE transactions
SET
    status = @Status,
    is_suspected_nc = @IsSuspectedNC,
    station = @Station,
    withdrawn_at = @WithdrawnAt,
    forpickup_at = @ForPickupAt,
    received_at = @ReceivedAt,
    consumed_at = @ConsumedAt,
    updated_at = CURRENT_TIMESTAMP
WHERE
    data_matrix = @DataMatrix;";


            var rows = connection.Execute(sql, transaction);

        }
    }
}