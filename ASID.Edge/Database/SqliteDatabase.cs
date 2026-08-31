using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace ASID.Edge.Database;

/// <summary>
/// Manages the local SQLite database used for offline-first transaction
/// storage.  The file lives in %LOCALAPPDATA%\ASID\asid_local.db.
/// </summary>
public static class SqliteDatabase
{
    private static readonly string DbDir =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ASID");

    private static readonly string DbPath =
        Path.Combine(DbDir, "asid_local.db");

    private static readonly string ConnectionString =
        $"Data Source={DbPath}";

    /// <summary>
    /// Create (or open) the local SQLite database and ensure the schema
    /// exists.  Called once at application startup.
    /// </summary>
    public static void Initialize()
    {
        Directory.CreateDirectory(DbDir);

        using var connection = CreateConnection();
        connection.Open();

        // Transactions table – mirrors the PostgreSQL transactions schema
        // but adds a `synced` flag so the background sync service knows
        // which rows still need to be pushed to the server.
        connection.Execute(@"
CREATE TABLE IF NOT EXISTS transactions
(
    id              TEXT    PRIMARY KEY,
    data_matrix     TEXT    NOT NULL UNIQUE,
    serial_no       TEXT    NOT NULL,
    model           TEXT    NOT NULL,
    part_no         TEXT    NOT NULL,
    quantity        INTEGER NOT NULL,
    kanban_no       TEXT    NOT NULL,
    operator_id     TEXT,
    line_no         TEXT,
    lane_no         TEXT,
    trolley_no      TEXT,
    station         TEXT    NOT NULL,
    status          TEXT    NOT NULL,
    created_at      TEXT,
    updated_at      TEXT,
    withdrawn_at    TEXT,
    forpickup_at    TEXT,
    received_at     TEXT,
    consumed_at     TEXT,
    is_suspected_nc INTEGER NOT NULL DEFAULT 0,
    is_nc_confirmed INTEGER NOT NULL DEFAULT 0,
    is_nc_rejected  INTEGER NOT NULL DEFAULT 0,
    nc_quantity     INTEGER NOT NULL DEFAULT 0,
    synced          INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_sqlite_tx_datamatrix
    ON transactions(data_matrix);

CREATE INDEX IF NOT EXISTS idx_sqlite_tx_synced
    ON transactions(synced);
");

        // Migrations: add columns that may be missing from older databases
        var columns = connection.Query<(
            int cid, string name, string type, int notnull,
            string? dflt_value, int pk)>(
            "PRAGMA table_info(transactions)")
            .Select(c => c.name.ToLowerInvariant())
            .ToList();

        void AddColumnIfMissing(string col, string ddl)
        {
            if (!columns.Contains(col.ToLowerInvariant()))
            {
                connection.Execute(ddl);
            }
        }

        AddColumnIfMissing("operator_id",
            "ALTER TABLE transactions ADD COLUMN operator_id TEXT");
        AddColumnIfMissing("is_suspected_nc",
            "ALTER TABLE transactions ADD COLUMN is_suspected_nc INTEGER NOT NULL DEFAULT 0");
        AddColumnIfMissing("is_nc_confirmed",
            "ALTER TABLE transactions ADD COLUMN is_nc_confirmed INTEGER NOT NULL DEFAULT 0");
        AddColumnIfMissing("is_nc_rejected",
            "ALTER TABLE transactions ADD COLUMN is_nc_rejected INTEGER NOT NULL DEFAULT 0");
        AddColumnIfMissing("nc_quantity",
            "ALTER TABLE transactions ADD COLUMN nc_quantity INTEGER NOT NULL DEFAULT 0");
        AddColumnIfMissing("synced",
            "ALTER TABLE transactions ADD COLUMN synced INTEGER NOT NULL DEFAULT 0");

        // Lane Management table
        connection.Execute(@"
CREATE TABLE IF NOT EXISTS lane_management
(
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    lane_no          TEXT    NOT NULL UNIQUE,
    part_no          TEXT    NOT NULL DEFAULT 'Not Assigned',
    max_qty_stored   INTEGER NOT NULL DEFAULT 100,
    actual_stored_qty INTEGER NOT NULL DEFAULT 0,
    withdrawn_qty    INTEGER NOT NULL DEFAULT 0,
    lane_status      TEXT    NOT NULL DEFAULT 'Not Assigned',
    color_status     TEXT    NOT NULL DEFAULT 'Gray',
    created_at       TEXT,
    updated_at       TEXT
);
");

        // Users table — local authentication so login works even when
        // the PostgreSQL server is unreachable.
        connection.Execute(@"
CREATE TABLE IF NOT EXISTS users
(
    id              TEXT    PRIMARY KEY,
    username        TEXT    NOT NULL UNIQUE,
    password_hash   TEXT    NOT NULL,
    role            TEXT    NOT NULL DEFAULT 'operator',
    created_at      TEXT,
    updated_at      TEXT
);
");

        // Seed default users if table is empty
        int userCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM users");
        if (userCount == 0)
        {
            var now = DateTime.UtcNow.ToString("o");
            var defaultUsers = new (string id, string username, string hash, string role)[]
            {
                (Guid.NewGuid().ToString(), "cordonez",
                 Services.PasswordHasher.Hash("4567"), "supervisor"),
                (Guid.NewGuid().ToString(), "rpingkian",
                 Services.PasswordHasher.Hash("1234"), "operator"),
                (Guid.NewGuid().ToString(), "admin",
                 Services.PasswordHasher.Hash("admin"), "qa"),
                (Guid.NewGuid().ToString(), "vsendrijas",
                 Services.PasswordHasher.Hash("7845"), "planner"),
            };

            foreach (var u in defaultUsers)
            {
                connection.Execute(@"
INSERT OR IGNORE INTO users (id, username, password_hash, role, created_at, updated_at)
VALUES (@Id, @Username, @Hash, @Role, @Now, @Now);",
                    new { u.id, u.username, u.hash, u.role, Now = now });
            }
        }
    }

    /// <summary>
    /// Create a new SQLite connection (caller disposes).
    /// </summary>
    public static SqliteConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }
}
