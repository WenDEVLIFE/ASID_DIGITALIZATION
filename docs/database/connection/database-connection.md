# Database Connection Documentation

This document describes the PostgreSQL database connection configuration for the **AZP Supermarket Inventory Digitalization (ASID)** system.

---

## 🔌 Connection Setup

> [!NOTE]
> The current connection points to a Neon cloud PostgreSQL instance for development/testing purposes. This will be replaced later with the actual on-premise database location.

### Connection Parameters

- **Database System**: PostgreSQL
- **Host Provider**: Neon (`aws.neon.tech` - `ap-southeast-1`)
- **Database Name**: `asid_db`
- **Port**: `5432`
- **SSL Mode**: `Require`
- **Channel Binding**: `Require`

---

## 💻 C# Implementation

The connection helper class uses [Npgsql](file:///e:/desktop_gui_projects/ASID/docs/database/connection/database-connection.md) to manage PostgreSQL connections.

```csharp
using Npgsql;

public static class Database
{
    private static readonly string ConnectionString =
        "Host=ep-shiny-shadow-ao74vw7s-pooler.c-2.ap-southeast-1.aws.neon.tech;" +
        "Port=5432;" +
        "Database=asid_db;" +
        "Username=neondb_owner;" +
        "Password=*****************;" +
        "SSL Mode=Require;" +
        "Channel Binding=Require;";

    public static NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(ConnectionString);
    }
}
```

---

## 🛡 Migration & Security Guidelines

1. **On-Premise Migration**: When deploying to on-premise infrastructure, update `ConnectionString` host, port, credentials, and SSL parameters accordingly (or load from app settings / environment variables).
2. **Connection Lifecycle**: Always wrap connections in `using` statements (or standard disposable blocks) to ensure connections are properly returned to the connection pool:
   ```csharp
   using var connection = Database.CreateConnection();
   connection.Open();
   // Execute operations...
   ```
