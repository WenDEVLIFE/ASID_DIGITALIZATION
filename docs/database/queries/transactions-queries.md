# Transactions Database Operations & Queries

This document details the SQL queries and C# Dapper data access code for managing records in the [`transactions`](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/transactions-table.md) table.

---

## 1. Add Transaction (Insert)

Inserts a new transaction record when a data matrix item is registered.

### C# Implementation

```csharp
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
```

---

## 2. Get Transaction by Data Matrix

Fetches a single transaction matching a specific `data_matrix` payload.

### C# Implementation

```csharp
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
```

---

## 3. Get All Transactions

Retrieves all recorded transactions.

### C# Implementation

```csharp
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
    created_at AS CreatedAt,
    withdrawn_at AS WithdrawnAt,
    forpickup_at AS ForPickupAt,
    received_at AS ReceivedAt,
    consumed_at AS ConsumedAt
FROM transactions;
")
.ToList();
```

---

## 4. Update Transaction Status & Lifecycles

Updates the `status`, `station`, lifecycle timestamps (`withdrawn_at`, `forpickup_at`, `received_at`, `consumed_at`), and `updated_at` timestamp.

### C# Implementation

```csharp
const string sql = @"
UPDATE transactions
SET
    status = @Status,
    station = @Station,
    withdrawn_at = @WithdrawnAt,
    forpickup_at = @ForPickupAt,
    received_at = @ReceivedAt,
    consumed_at = @ConsumedAt,
    updated_at = CURRENT_TIMESTAMP
WHERE
    data_matrix = @DataMatrix;";

var rows = connection.Execute(sql, transaction);
```
