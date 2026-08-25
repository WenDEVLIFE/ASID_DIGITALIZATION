/*
===============================================================================
 ASID - Microsoft SQL Server 2022 Schema
===============================================================================
 Purpose : Create the ASID tables on an end-user MSSQL 2022 instance so that
           ASID data can be passed/received through their API.
 Source  : Mirrors the ASID PostgreSQL schema (asid_db) - table & column
           names are kept IDENTICAL for API compatibility.
 Notes   :
   - MSSQL `TIMESTAMP` is a ROWVERSION type, NOT a date. For date columns we
     use DATETIME2 (and DATE for date-only columns).
   - UUID  -> UNIQUEIDENTIFIER (NEWID() generates a new value per row).
   - TEXT  -> NVARCHAR(MAX) to keep the unbounded behaviour of Postgres TEXT.
   - BOOLEAN -> BIT.
   - BIGSERIAL -> BIGINT IDENTITY(1,1).
 Run     : Execute against the target database on SQL Server 2022.
===============================================================================
*/

-- ===========================================================================
-- transactions
-- ===========================================================================
IF OBJECT_ID(N'dbo.transactions', N'U') IS NOT NULL
    DROP TABLE dbo.transactions;
GO

CREATE TABLE dbo.transactions
(
    id              UNIQUEIDENTIFIER NOT NULL
                        CONSTRAINT PK_transactions PRIMARY KEY
                        DEFAULT NEWID(),

    data_matrix     NVARCHAR(MAX)    NOT NULL,
    serial_no       NVARCHAR(MAX)    NOT NULL,

    model           NVARCHAR(MAX)    NOT NULL,
    part_no         NVARCHAR(MAX)    NOT NULL,
    quantity        INT              NOT NULL,
    kanban_no       NVARCHAR(MAX)    NOT NULL,

    operator_id     NVARCHAR(MAX)    NULL,
    line_no         NVARCHAR(MAX)    NULL,
    lane_no         NVARCHAR(MAX)    NULL,
    trolley_no      NVARCHAR(MAX)    NULL,

    station         NVARCHAR(MAX)    NOT NULL,
    status          NVARCHAR(MAX)    NOT NULL,

    created_at      DATETIME2        NULL
                        CONSTRAINT DF_transactions_created_at DEFAULT SYSUTCDATETIME(),
    updated_at      DATETIME2        NULL
                        CONSTRAINT DF_transactions_updated_at DEFAULT SYSUTCDATETIME(),
    withdrawn_at    DATETIME2        NULL,
    forpickup_at    DATETIME2        NULL,
    received_at     DATETIME2        NULL,
    consumed_at     DATETIME2        NULL,

    is_suspected_nc BIT              NOT NULL
                        CONSTRAINT DF_transactions_is_suspected_nc DEFAULT 0
);
GO

-- data_matrix is UNIQUE in the source schema (barcode payloads must not repeat).
CREATE UNIQUE INDEX UQ_transactions_data_matrix
    ON dbo.transactions(data_matrix);
GO

-- Performance indexes (mirror the documented indexes).
CREATE INDEX idx_transaction_datamatrix
    ON dbo.transactions(data_matrix);
GO

CREATE INDEX idx_transaction_status
    ON dbo.transactions(status);
GO

-- ===========================================================================
-- daily_demand
-- ===========================================================================
IF OBJECT_ID(N'dbo.daily_demand', N'U') IS NOT NULL
    DROP TABLE dbo.daily_demand;
GO

CREATE TABLE dbo.daily_demand
(
    id              BIGINT IDENTITY(1,1) NOT NULL
                        CONSTRAINT PK_daily_demand PRIMARY KEY,
    production_date DATE       NOT NULL,
    shift           SMALLINT   NOT NULL,
    model           NVARCHAR(100) NULL,
    part_no         NVARCHAR(100) NOT NULL,
    quantity        INT        NOT NULL,
    scrapped        INT        NOT NULL DEFAULT 0,
    imported_at     DATETIME2  NULL
                        CONSTRAINT DF_daily_demand_imported_at DEFAULT SYSUTCDATETIME()
);
GO

-- ===========================================================================
-- users
-- ===========================================================================
IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
    DROP TABLE dbo.users;
GO

CREATE TABLE dbo.users
(
    id            UNIQUEIDENTIFIER NOT NULL
                        CONSTRAINT PK_users PRIMARY KEY
                        DEFAULT NEWID(),
    username      NVARCHAR(MAX)    NOT NULL,
    password_hash NVARCHAR(MAX)    NOT NULL,
    role          NVARCHAR(MAX)    NOT NULL,
    created_at    DATETIME2        NULL
                        CONSTRAINT DF_users_created_at DEFAULT SYSUTCDATETIME(),
    updated_at    DATETIME2        NULL
                        CONSTRAINT DF_users_updated_at DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX UQ_users_username
    ON dbo.users(username);
GO

CREATE INDEX idx_users_username
    ON dbo.users(username);
GO

-- Seed users (dev-only) — mirrors the PostgreSQL seed.
INSERT INTO dbo.users (username, password_hash, role) VALUES
 ('rpingkian','100000.cnBpbmdraWFuLnNlZWQuMQ==.puzn8+C7JXIRWavL9JoOI69hM2MCqKX5s4pT2Gnp+h0=','operator'),
 ('vsendrijas','100000.dnNlbmRyaWphcy5zZWVkMg==.EFGM8CjsmzidgpiLRV4tBOy1lR9DWp6k7iZuouNklRo=','qa'),
 ('cordonez','100000.Y29yZG9uZXouc2VlZC4wMw==.aFngONRDfMiKH7HQnepsFBr+btK6oMo5m8x5xZQ6Syc=','supervisor');
GO
