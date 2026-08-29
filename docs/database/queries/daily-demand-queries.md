# Daily Demand Database Operations & Queries

This document details the SQL queries and C# Dapper data access code for managing records in the [`daily_demand`](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/daily-demand-table.md) table.

---

## 1. Insert Daily Demand

Inserts daily demand schedule entries.

### C# Implementation

```csharp
const string sql = @"
INSERT INTO daily_demand
(
    production_date,
    shift,
    model,
    part_no,
    quantity,
    scrapped,
    imported_at
)
VALUES
(
    @ProductionDate,
    @Shift,
    @Model,
    @PartNo,
    @Quantity,
    @Scrapped,
    @ImportedAt
);";

connection.Execute(sql, demands);
```

---

## 2. Get Daily Demand by Date

Queries daily demand records for a specific date, ordered by `part_no` and `shift`.

### C# Implementation

```csharp
const string sql = @"
SELECT
    id AS Id,
    CAST(production_date AS timestamp) AS ProductionDate,
    shift AS Shift,
    model AS Model,
    part_no AS PartNo,
    quantity AS Quantity,
    scrapped AS Scrapped,
    imported_at AS ImportedAt
FROM daily_demand
WHERE production_date = @date
ORDER BY
    part_no,
    shift;";

return connection.Query<DailyDemand>(
    sql,
    new { date })
.ToList();
```

---

## 3. Delete All Daily Demand

Truncates or removes all rows from `daily_demand`.

### C# Implementation

```csharp
public void DeleteAll()
{
    using var connection = Database.Database.CreateConnection();
    connection.Open();
    connection.Execute("DELETE FROM daily_demand;");
}
```

---

## 4. Delete Daily Demand by Workweek

Removes demand records for a specific production week (used for mid-week updates).

### C# Implementation

```csharp
public void DeleteByWorkweek(DateTime weekStart)
{
    using var connection = Database.Database.CreateConnection();
    connection.Open();
    connection.Execute(
        "DELETE FROM daily_demand WHERE production_date = @weekStart;",
        new { weekStart });
}
```

---

## 5. Get Last Imported Timestamp

Returns the most recent `imported_at` value across all daily demand records.
Used for change detection (detecting when the planner uploads a new file).

### C# Implementation

```csharp
public DateTime? GetLastImportedAt()
{
    using var connection = Database.Database.CreateConnection();
    connection.Open();
    return connection.QuerySingleOrDefault<DateTime?>(
        "SELECT MAX(imported_at) FROM daily_demand;");
}
```
