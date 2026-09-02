/*
===============================================================================
 ASID - Microsoft SQL Server 2022 Schema
===============================================================================
 Purpose : Create the ASID tables on an end-user MSSQL 2022 instance so that
           ASID data can be passed/received through their API.

 Source  : Mirrors the ASID PostgreSQL schema.
           Table and column names are kept IDENTICAL for API compatibility.

 MSSQL Mapping:
   UUID       -> UNIQUEIDENTIFIER
   TEXT       -> NVARCHAR(MAX) where not indexed
   TEXT       -> bounded NVARCHAR where indexed
   BOOLEAN    -> BIT
   BIGSERIAL  -> BIGINT IDENTITY(1,1)
   TIMESTAMP  -> DATETIME2
   DATE       -> DATE

 Important:
   - SQL Server cannot use NVARCHAR(MAX) as an index key.
   - Therefore, indexed/unique text columns use bounded NVARCHAR sizes.
   - UNIQUE indexes already provide lookup capability, so redundant
     non-unique indexes on the same column have been removed.

 Run against:
   Microsoft SQL Server 2022
===============================================================================
*/


/*
===============================================================================
 1. TRANSACTIONS
===============================================================================
*/

IF OBJECT_ID(N'dbo.transactions', N'U') IS NOT NULL
    DROP TABLE dbo.transactions;
GO


CREATE TABLE dbo.transactions
(
    id              UNIQUEIDENTIFIER NOT NULL
                        CONSTRAINT PK_transactions
                        PRIMARY KEY
                        DEFAULT NEWID(),

    /*
       Indexed + UNIQUE.
       NVARCHAR(MAX) cannot be an index key in SQL Server.
       500 characters gives sufficient room for barcode/data-matrix payloads.
    */
    data_matrix     NVARCHAR(500)    NOT NULL,

    serial_no       NVARCHAR(100)    NOT NULL,

    model           NVARCHAR(100)    NOT NULL,

    part_no         NVARCHAR(100)    NOT NULL,

    quantity        INT              NOT NULL,

    kanban_no       NVARCHAR(100)    NOT NULL,

    operator_id     NVARCHAR(100)    NULL,

    line_no         NVARCHAR(50)     NULL,

    lane_no         NVARCHAR(50)     NULL,

    trolley_no      NVARCHAR(50)     NULL,

    station         NVARCHAR(100)    NOT NULL,

    /*
       Indexed.
    */
    status          NVARCHAR(50)     NOT NULL,

    created_at      DATETIME2        NULL
                        CONSTRAINT DF_transactions_created_at
                        DEFAULT SYSUTCDATETIME(),

    updated_at      DATETIME2        NULL
                        CONSTRAINT DF_transactions_updated_at
                        DEFAULT SYSUTCDATETIME(),

    withdrawn_at    DATETIME2        NULL,

    forpickup_at    DATETIME2        NULL,

    received_at     DATETIME2        NULL,

    consumed_at     DATETIME2        NULL,

    is_suspected_nc BIT              NOT NULL
                        CONSTRAINT DF_transactions_is_suspected_nc
                        DEFAULT 0,

    is_nc_confirmed BIT              NOT NULL
                        CONSTRAINT DF_transactions_is_nc_confirmed
                        DEFAULT 0,

    is_nc_rejected  BIT              NOT NULL
                        CONSTRAINT DF_transactions_is_nc_rejected
                        DEFAULT 0,

    nc_quantity     INT              NOT NULL
                        CONSTRAINT DF_transactions_nc_quantity
                        DEFAULT 0
);
GO


/*
===============================================================================
 UNIQUE DATA MATRIX
===============================================================================

 Source requirement:
   data_matrix must not be duplicated.

 UNIQUE INDEX also provides efficient lookup by data_matrix.
*/
CREATE UNIQUE INDEX UQ_transactions_data_matrix
    ON dbo.transactions(data_matrix);
GO


/*
===============================================================================
 STATUS INDEX
===============================================================================
*/
CREATE INDEX IX_transactions_status
    ON dbo.transactions(status);
GO



/*
===============================================================================
 2. DAILY DEMAND
===============================================================================
*/

IF OBJECT_ID(N'dbo.daily_demand', N'U') IS NOT NULL
    DROP TABLE dbo.daily_demand;
GO


CREATE TABLE dbo.daily_demand
(
    id              BIGINT IDENTITY(1,1) NOT NULL
                        CONSTRAINT PK_daily_demand
                        PRIMARY KEY,

    production_date DATE              NOT NULL,

    shift           SMALLINT          NOT NULL,

    model           NVARCHAR(100)     NULL,

    part_no         NVARCHAR(100)     NOT NULL,

    quantity        INT               NOT NULL,

    scrapped        INT               NOT NULL
                        CONSTRAINT DF_daily_demand_scrapped
                        DEFAULT 0,

    imported_at     DATETIME2         NULL
                        CONSTRAINT DF_daily_demand_imported_at
                        DEFAULT SYSUTCDATETIME()
);
GO



/*
===============================================================================
 3. USERS
===============================================================================
*/

IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
    DROP TABLE dbo.users;
GO


CREATE TABLE dbo.users
(
    id            UNIQUEIDENTIFIER NOT NULL
                    CONSTRAINT PK_users
                    PRIMARY KEY
                    DEFAULT NEWID(),

    /*
       Indexed + UNIQUE.
       NVARCHAR(MAX) cannot be used as an index key.
    */
    username      NVARCHAR(100) NOT NULL,

    /*
       Password hashes do not need to be indexed.
       Keep MAX to avoid unnecessarily restricting the hash format.
    */
    password_hash NVARCHAR(MAX) NOT NULL,

    role          NVARCHAR(50) NOT NULL,

    created_at    DATETIME2 NULL
                    CONSTRAINT DF_users_created_at
                    DEFAULT SYSUTCDATETIME(),

    updated_at    DATETIME2 NULL
                    CONSTRAINT DF_users_updated_at
                    DEFAULT SYSUTCDATETIME()
);
GO


/*
===============================================================================
 UNIQUE USERNAME
===============================================================================

 This also acts as the lookup index for username.
 No additional idx_users_username is necessary.
*/
CREATE UNIQUE INDEX UQ_users_username
    ON dbo.users(username);
GO



/*
===============================================================================
 4. SEED USERS
===============================================================================

 DEV-ONLY USERS

 rpingkian    / 1234  -> Operator
 vsendrijas   / 5678  -> QA
 cordonez     / 4567  -> Supervisor
 vsendrijas.p / 7845 -> Planner

 NOTE:
 These are existing password hashes from the PostgreSQL seed.
 They are NOT plaintext passwords stored in the database.
===============================================================================
*/

INSERT INTO dbo.users
(
    username,
    password_hash,
    role
)
VALUES
(
    'rpingkian',
    '100000./PzwRW3iWSQO6ocYyOAmqg==.Qi5DSaA2N6KsFkFHBTRv4qLqr0LmgFPezE3XEZHPFks=',
    'operator'
),
(
    'vsendrijas',
    '100000.7gP6pxaWtEshcZDqHig5eQ==.qi4uZgj6uPjiLL/DJBKuVd89i5HDhjLlgRlN5Da4M3s=',
    'qa'
),
(
    'cordonez',
    '100000.BPGX/TrkXhsabeg3clB4WA==.+iRnOiyGJT52s0zPSWpxTCBd1PgisUgp7DwJp5Rx6H0=',
    'supervisor'
),
(
    'vsendrijas.p',
    '100000.0swiOwjI3eKKwLGNP5dmXQ==.I4hbwy+svxcKjTXKWwma1PFHwfwaiPE/516bAv2jWWE=',
    'planner'
);
GO



/*
===============================================================================
 5. LANE MANAGEMENT
===============================================================================
*/

IF OBJECT_ID(N'dbo.lane_management', N'U') IS NOT NULL
    DROP TABLE dbo.lane_management;
GO


CREATE TABLE dbo.lane_management
(
    id                INT IDENTITY(1,1) NOT NULL
                            CONSTRAINT PK_lane_management
                            PRIMARY KEY,

    lane_no           NVARCHAR(50)  NOT NULL,

    part_no           NVARCHAR(100) NOT NULL
                            CONSTRAINT DF_lane_management_part_no
                            DEFAULT 'Not Assigned',

    max_qty_stored    INT           NOT NULL
                            CONSTRAINT DF_lane_management_max_qty_stored
                            DEFAULT 100,

    actual_stored_qty INT           NOT NULL
                            CONSTRAINT DF_lane_management_actual_stored_qty
                            DEFAULT 0,

    withdrawn_qty     INT           NOT NULL
                            CONSTRAINT DF_lane_management_withdrawn_qty
                            DEFAULT 0,

    lane_status       NVARCHAR(50)  NOT NULL
                            CONSTRAINT DF_lane_management_lane_status
                            DEFAULT 'Not Assigned',

    color_status      NVARCHAR(50)  NOT NULL
                            CONSTRAINT DF_lane_management_color_status
                            DEFAULT 'Gray',

    created_at        DATETIME2     NULL
                            CONSTRAINT DF_lane_management_created_at
                            DEFAULT SYSUTCDATETIME(),

    updated_at        DATETIME2     NULL
                            CONSTRAINT DF_lane_management_updated_at
                            DEFAULT SYSUTCDATETIME()
);
GO


/*
===============================================================================
 UNIQUE LANE NUMBER
===============================================================================
*/

CREATE UNIQUE INDEX UQ_lane_management_lane_no
    ON dbo.lane_management(lane_no);
GO



/*
===============================================================================
 OPTIONAL VERIFICATION
===============================================================================

 These queries allow you to verify that the tables and indexes were created.
===============================================================================
*/

SELECT
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_NAME IN
  (
      'transactions',
      'daily_demand',
      'users',
      'lane_management'
  )
ORDER BY TABLE_NAME;
GO


SELECT
    t.name AS table_name,
    i.name AS index_name,
    i.type_desc AS index_type,
    i.is_unique
FROM sys.indexes i
INNER JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE t.name IN
(
    'transactions',
    'daily_demand',
    'users',
    'lane_management'
)
AND i.name IS NOT NULL
ORDER BY
    t.name,
    i.name;
GO