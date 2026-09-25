/*
===============================================================================
 Migration: Add plant_return table
===============================================================================
 Purpose : Creates dbo.plant_return so plant returns can be recorded and their
           quantities absorbed into the current-week Scrapped value on the
           PU-Body dashboard.
 Run     : Execute against your live MSSQL database.
           Safe to re-run: the table is only created when it does not exist,
           and no existing data is dropped.
===============================================================================
*/

IF OBJECT_ID(N'dbo.plant_return', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.plant_return
    (
        id              UNIQUEIDENTIFIER NOT NULL
                            CONSTRAINT PK_plant_return
                            PRIMARY KEY
                            DEFAULT NEWID(),

        part_no         NVARCHAR(100)    NOT NULL,

        quantity        INT              NOT NULL,

        remarks         NVARCHAR(500)    NULL,

        /*
           Username of the user who authorized the return.
        */
        username        NVARCHAR(100)    NULL,

        created_at      DATETIME2        NOT NULL
                            CONSTRAINT DF_plant_return_created_at
                            DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_plant_return_part_no_created_at
        ON dbo.plant_return(part_no, created_at);
END
GO

-- Verify
SELECT
    t.name AS table_name,
    i.name AS index_name
FROM sys.indexes i
INNER JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE t.name = 'plant_return'
  AND i.name IS NOT NULL
ORDER BY i.name;
GO
