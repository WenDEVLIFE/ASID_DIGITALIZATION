/*
================================================================================
 ASID - Dummy Seed Data for dbo.transactions (SQL Server 2022)
================================================================================
 Purpose : Insert dummy rows so a tester can verify that the three dashboard
           grids populate data:
             - "Transaction History"
             - "PU-BODY INVENTORY"
             - "PU-BODY WITHDRAWAL"

 These three grids are NOT separate tables - they are projections of
 dbo.transactions computed by the ASID.Edge mappers.

 Status string -> inventory column mapping (from InventoryMapper.cs):
     'Stored'     -> InventoryP2Supermarket
     'Withdrawn'  -> InventoryFloating
     'ForPickup'  -> InventoryP2LoadingBay
     'Received'   -> InventoryP1LoadingBay
     'Consumed'   -> InventoryP1Production

 Withdrawal grid (WithdrawalMapper.cs):
     Rows where withdrawn_at IS NOT NULL.

 History grid (TransactionHistoryMapper.cs):
     All rows, ordered by created_at DESC (TOP 20 in the app).

 Safe to re-run: it deletes prior DUMMY- rows, then inserts fresh ones.
 No real credentials are used.
================================================================================
*/

SET NOCOUNT ON;
GO


/*
================================================================================
 1. CLEAN PREVIOUS DUMMY ROWS (idempotency)
================================================================================
 Removes any rows previously created by this script so it can be re-run safely.
 Rows are matched by the DUMMY- data_matrix prefix (unique index key).
================================================================================
*/
DELETE FROM dbo.transactions
WHERE data_matrix LIKE 'DUMMY-%';
GO


/*
================================================================================
 2. INSERT DUMMY ROWS
================================================================================
 All status strings below match MaterialStatus.ToString() exactly as persisted
 by the app. Column list is explicit (not positional).

 Timestamps are recent (relative to today) and use SYSDATETIME() so the grids
 show fresh, descending order. Each lifecycle status sets its matching
 timestamp column:
     'Stored'    -> (created_at only)
     'Withdrawn' -> withdrawn_at
     'ForPickup' -> forpickup_at
     'Received'  -> received_at
     'Consumed'  -> consumed_at
================================================================================
*/

INSERT INTO dbo.transactions
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
    status,
    created_at,
    updated_at,
    withdrawn_at,
    forpickup_at,
    received_at,
    consumed_at,
    is_suspected_nc,
    is_nc_confirmed,
    is_nc_rejected,
    nc_quantity
)
VALUES

-- ============================================================
-- STORED (supermarket) - fills InventoryP2Supermarket
-- ============================================================
(
    'DUMMY-STORED-001',
    'SRL-1001',
    'PU-BODY',
    'PU-BODY-001',
    60,
    'KAN-0001',
    'OP-001',
    'LINE-1',
    'LN-A1',
    'TR-101',
    'ST001',
    'Stored',
    SYSDATETIME(),
    SYSDATETIME(),
    NULL,
    NULL,
    NULL,
    NULL,
    0,
    0,
    0,
    0
),
(
    'DUMMY-STORED-002',
    'SRL-1002',
    'PU-BODY',
    'PU-BODY-001',
    40,
    'KAN-0002',
    'OP-001',
    'LINE-1',
    'LN-A2',
    'TR-102',
    'ST001',
    'Stored',
    SYSDATETIME(),
    SYSDATETIME(),
    NULL,
    NULL,
    NULL,
    NULL,
    0,
    0,
    0,
    0
),

-- ============================================================
-- WITHDRAWN (floating) - fills InventoryFloating + Withdrawal grid
-- ============================================================
(
    'DUMMY-WITHDRAWN-001',
    'SRL-2001',
    'PU-BODY',
    'PU-BODY-002',
    25,
    'KAN-0003',
    'OP-002',
    'LINE-2',
    'LN-B1',
    'TR-201',
    'ST002',
    'Withdrawn',
    DATEADD(HOUR, -3, SYSDATETIME()),
    DATEADD(HOUR, -2, SYSDATETIME()),
    DATEADD(HOUR, -2, SYSDATETIME()),
    NULL,
    NULL,
    NULL,
    0,
    0,
    0,
    0
),

-- ============================================================
-- FORPICKUP (P2 loading bay) - fills InventoryP2LoadingBay
-- ============================================================
(
    'DUMMY-FORPICKUP-001',
    'SRL-3001',
    'PU-BODY',
    'PU-BODY-003',
    18,
    'KAN-0004',
    'OP-003',
    'LINE-3',
    'LN-C1',
    'TR-301',
    'ST003',
    'ForPickup',
    DATEADD(HOUR, -6, SYSDATETIME()),
    DATEADD(HOUR, -5, SYSDATETIME()),
    NULL,
    DATEADD(HOUR, -5, SYSDATETIME()),
    NULL,
    NULL,
    0,
    0,
    0,
    0
),

-- ============================================================
-- RECEIVED (P1 loading bay) - fills InventoryP1LoadingBay
-- ============================================================
(
    'DUMMY-RECEIVED-001',
    'SRL-4001',
    'PU-BODY',
    'PU-BODY-004',
    12,
    'KAN-0005',
    'OP-004',
    'LINE-4',
    'LN-D1',
    'TR-401',
    'ST004',
    'Received',
    DATEADD(HOUR, -9, SYSDATETIME()),
    DATEADD(HOUR, -8, SYSDATETIME()),
    NULL,
    NULL,
    DATEADD(HOUR, -8, SYSDATETIME()),
    NULL,
    0,
    0,
    0,
    0
),

-- ============================================================
-- CONSUMED (P1 production) - fills InventoryP1Production
-- ============================================================
(
    'DUMMY-CONSUMED-001',
    'SRL-5001',
    'PU-BODY',
    'PU-BODY-005',
    30,
    'KAN-0006',
    'OP-005',
    'LINE-5',
    'LN-E1',
    'TR-501',
    'ST005',
    'Consumed',
    DATEADD(HOUR, -12, SYSDATETIME()),
    DATEADD(HOUR, -11, SYSDATETIME()),
    NULL,
    NULL,
    NULL,
    DATEADD(HOUR, -11, SYSDATETIME()),
    0,
    0,
    0,
    0
),

-- ============================================================
-- SCRAPPED - excluded from all inventory columns (sanity only)
-- ============================================================
(
    'DUMMY-SCRAPPED-001',
    'SRL-6001',
    'PU-BODY',
    'PU-BODY-006',
    5,
    'KAN-0007',
    'OP-001',
    'LINE-1',
    'LN-F1',
    'TR-601',
    'ST001',
    'Scrapped',
    DATEADD(HOUR, -15, SYSDATETIME()),
    DATEADD(HOUR, -14, SYSDATETIME()),
    NULL,
    NULL,
    NULL,
    NULL,
    0,
    0,
    0,
    0
),

-- ============================================================
-- SUSPECTED NC / CONFIRMED NC - still appears in history grid
-- ============================================================
(
    'DUMMY-NC-001',
    'SRL-7001',
    'PU-BODY',
    'PU-BODY-007',
    20,
    'KAN-0008',
    'OP-002',
    'LINE-2',
    'LN-G1',
    'TR-701',
    'ST002',
    'Received',
    DATEADD(HOUR, -18, SYSDATETIME()),
    DATEADD(HOUR, -17, SYSDATETIME()),
    NULL,
    NULL,
    DATEADD(HOUR, -17, SYSDATETIME()),
    NULL,
    1,
    1,
    0,
    8
);
GO


/*
================================================================================
 3. VERIFICATION QUERIES
================================================================================
 These mirror what the ASID.Edge grids display, so the tester can confirm the
 seed produced data in every grid.
================================================================================
*/


-- 3a. QUICK SANITY COUNT (status distribution)
-- ----------------------------------------------------------------
SELECT
    status,
    COUNT(*) AS row_count
FROM dbo.transactions
WHERE data_matrix LIKE 'DUMMY-%'
GROUP BY status
ORDER BY status;
GO


-- 3b. TRANSACTION HISTORY
-- ----------------------------------------------------------------
-- Mirrors TransactionHistoryMapper: all rows, newest first.
SELECT TOP 20
    CONVERT(NVARCHAR(10), created_at, 23) AS [Date],
    CONVERT(NVARCHAR(8),  created_at, 108) AS [Time],
    model,
    part_no,
    quantity   AS SNP,
    serial_no,
    line_no,
    lane_no,
    trolley_no,
    operator_id,
    status,
    is_suspected_nc,
    is_nc_confirmed,
    is_nc_rejected,
    nc_quantity
FROM dbo.transactions
WHERE data_matrix LIKE 'DUMMY-%'
ORDER BY created_at DESC;
GO


-- 3c. PU-BODY INVENTORY (conditional aggregation per InventoryMapper)
-- ----------------------------------------------------------------
-- Status -> column mapping:
--   'Stored'     -> P2 Supermarket
--   'Withdrawn'  -> Floating
--   'ForPickup'  -> P2 Loading Bay
--   'Received'   -> P1 Loading Bay
--   'Consumed'   -> P1 Production
SELECT
    model,
    part_no,
    SUM(CASE WHEN status = 'Stored'    THEN quantity ELSE 0 END) AS InventoryP2Supermarket,
    SUM(CASE WHEN status = 'Withdrawn' THEN quantity ELSE 0 END) AS InventoryFloating,
    SUM(CASE WHEN status = 'ForPickup' THEN quantity ELSE 0 END) AS InventoryP2LoadingBay,
    SUM(CASE WHEN status = 'Received'  THEN quantity ELSE 0 END) AS InventoryP1LoadingBay,
    SUM(CASE WHEN status = 'Consumed'  THEN quantity ELSE 0 END) AS InventoryP1Production
FROM dbo.transactions
WHERE data_matrix LIKE 'DUMMY-%'
GROUP BY model, part_no
ORDER BY model, part_no;
GO


-- 3d. PU-BODY WITHDRAWAL
-- ----------------------------------------------------------------
-- Mirrors WithdrawalMapper: rows where withdrawn_at IS NOT NULL, newest first.
SELECT
    CONVERT(NVARCHAR(10), withdrawn_at, 23) AS [Date],
    CONVERT(NVARCHAR(8),  withdrawn_at, 108) AS [Time],
    model,
    part_no,
    trolley_no,
    quantity AS SNP
FROM dbo.transactions
WHERE data_matrix LIKE 'DUMMY-%'
  AND withdrawn_at IS NOT NULL
ORDER BY withdrawn_at DESC;
GO


PRINT 'Seed complete. Run the VERIFICATION queries above to confirm the grids.';
GO