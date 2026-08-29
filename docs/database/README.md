# AZP Supermarket Inventory Digitalization (ASID) - Database Overview

Welcome to the **ASID PostgreSQL Database** documentation. This section covers the database architecture, schema definitions, connection configurations, and data access layers for the AZP Supermarket Inventory Digitalization system.

## 📂 Documentation Structure

The database documentation is organized into dedicated subfolders for clarity and maintainability:

```text
docs/database/
├── README.md                           <- You are here (Database Overview)
├── connection/
│   └── database-connection.md          <- Connection Setup & C# Helper
├── schemas/
│   ├── transactions-table.md           <- Schema for 'transactions' table
│   ├── daily-demand-table.md           <- Schema for 'daily_demand' table
│   └── users-table.md                  <- Schema for 'users' table (login & RBAC)
└── queries/
    ├── transactions-queries.md         <- Queries & C# Data Access for 'transactions'
    └── daily-demand-queries.md         <- Queries & C# Data Access for 'daily_demand'
```

---

## 📊 Database Summary

- **DBMS**: PostgreSQL
- **Database Name**: `asid_db`
- **Data Access Library**: Npgsql + Dapper (C#)

### Core Tables

| Table Name | Description | Key Column | Indexes |
| :--- | :--- | :--- | :--- |
| [`transactions`](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/transactions-table.md) | Tracks data matrix scanning lifecycle, stations, timestamps, and inventory status. | `id` (UUID) | `idx_transaction_datamatrix`, `idx_transaction_status` |
| [`daily_demand`](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/daily-demand-table.md) | Stores daily production demand records per shift, model, and part number. | `id` (BIGSERIAL) | Primary Key (`id`) |
| [`users`](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/users-table.md) | Stores login credentials (PBKDF2 hashes) and RBAC roles. | `id` (UUID) | `UNIQUE` (`username`), `idx_users_username` |

---

## 🛠 Getting Started

1. **Connection Details**: Review [database-connection.md](file:///e:/desktop_gui_projects/ASID/docs/database/connection/database-connection.md) to inspect the C# connection helper and Neon database configuration.
2. **Schema Reference**: Check [transactions-table.md](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/transactions-table.md), [daily-demand-table.md](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/daily-demand-table.md), and [users-table.md](file:///e:/desktop_gui_projects/ASID/docs/database/schemas/users-table.md) for full DDL and column descriptions.
3. **Query Implementations**: Refer to [transactions-queries.md](file:///e:/desktop_gui_projects/ASID/docs/database/queries/transactions-queries.md) and [daily-demand-queries.md](file:///e:/desktop_gui_projects/ASID/docs/database/queries/daily-demand-queries.md) for inline SQL and Dapper query mappings.
