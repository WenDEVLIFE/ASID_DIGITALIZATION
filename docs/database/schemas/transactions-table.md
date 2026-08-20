# Transactions Table Schema

The `transactions` table tracks data matrix barcode scanning events, kanban assignments, station progression, lifecycle timestamps, and inventory status within the ASID system.

---

## 📜 SQL DDL Script

```sql
CREATE TABLE transactions
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    data_matrix TEXT NOT NULL UNIQUE,
    serial_no TEXT NOT NULL,
    model TEXT NOT NULL,
    part_no TEXT NOT NULL,
    quantity INTEGER NOT NULL,
    kanban_no TEXT NOT NULL,
    operator_id TEXT,
    line_no TEXT,
    lane_no TEXT,
    trolley_no TEXT,
    station TEXT NOT NULL,
    status TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    withdrawn_at TIMESTAMP,
    forpickup_at TIMESTAMP,
    received_at TIMESTAMP,
    consumed_at TIMESTAMP
);

-- Performance Indexes
CREATE INDEX idx_transaction_datamatrix
    ON transactions(data_matrix);

CREATE INDEX idx_transaction_status
    ON transactions(status);
```

---

## 📋 Column Specifications

| Column Name | Data Type | Nullable | Constraints / Default | Description |
| :--- | :--- | :---: | :--- | :--- |
| `id` | `UUID` | No | `PRIMARY KEY`, `gen_random_uuid()` | Unique transaction surrogate key. |
| `data_matrix` | `TEXT` | No | `UNIQUE` | Scanned data matrix barcode value. |
| `serial_no` | `TEXT` | No | - | Serial number of the unit/part. |
| `model` | `TEXT` | No | - | Vehicle / part model designation. |
| `part_no` | `TEXT` | No | - | Component part number. |
| `quantity` | `INTEGER` | No | - | SNP / Batch quantity per transaction. |
| `kanban_no` | `TEXT` | No | - | Kanban identification code. |
| `operator_id` | `TEXT` | Yes | - | Identifier of the operator handling the item. |
| `line_no` | `TEXT` | Yes | - | Production line identifier. |
| `lane_no` | `TEXT` | Yes | - | Supermarket lane identifier. |
| `trolley_no` | `TEXT` | Yes | - | Transport trolley identifier. |
| `station` | `TEXT` | No | - | Current processing station name/ID. |
| `status` | `TEXT` | No | - | Current lifecycle status string. |
| `created_at` | `TIMESTAMP` | Yes | `CURRENT_TIMESTAMP` | Timestamp when the transaction record was generated. |
| `updated_at` | `TIMESTAMP` | Yes | `CURRENT_TIMESTAMP` | Timestamp when the transaction record was last updated. |
| `withdrawn_at` | `TIMESTAMP` | Yes | - | Timestamp when material was withdrawn. |
| `forpickup_at` | `TIMESTAMP` | Yes | - | Timestamp when material was marked ready for pickup. |
| `received_at` | `TIMESTAMP` | Yes | - | Timestamp when material was received at target station. |
| `consumed_at` | `TIMESTAMP` | Yes | - | Timestamp when material was consumed on the line. |

---

## ⚡ Indexes

- **`idx_transaction_datamatrix`**: Single-column index on `data_matrix` for fast lookups by barcode payload.
- **`idx_transaction_status`**: Single-column index on `status` to optimize filtering active vs completed transactions.
