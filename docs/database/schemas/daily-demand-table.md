# Daily Demand Table Schema

The `daily_demand` table stores daily production demand schedules per shift, model, and part number in the ASID system.

---

## 📜 SQL DDL Script

```sql
CREATE TABLE daily_demand
(
    id BIGSERIAL PRIMARY KEY,
    production_date DATE NOT NULL,
    shift SMALLINT NOT NULL,
    model VARCHAR(100),
    part_no VARCHAR(100) NOT NULL,
    quantity INTEGER NOT NULL
);
```

---

## 📋 Column Specifications

| Column Name | Data Type | Nullable | Constraints / Default | Description |
| :--- | :--- | :---: | :--- | :--- |
| `id` | `BIGSERIAL` | No | `PRIMARY KEY` | Auto-incrementing 64-bit identifier. |
| `production_date` | `DATE` | No | - | Production schedule target date (`YYYY-MM-DD`). |
| `shift` | `SMALLINT` | No | - | Production shift number (e.g. `1`, `2`, `3`). |
| `model` | `VARCHAR(100)` | Yes | - | Target vehicle or component model. |
| `part_no` | `VARCHAR(100)` | No | - | Component part number. |
| `quantity` | `INTEGER` | No | - | Target required production quantity. |

---

## 🔗 Related Operations

For query routines involving `daily_demand`, check [daily-demand-queries.md](file:///e:/desktop_gui_projects/ASID/docs/database/queries/daily-demand-queries.md).
