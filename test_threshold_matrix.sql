/*
================================================================================
 ASID - Manual Runtime Test Matrix: Daily Demand Variance Threshold (T4b/T4c)
================================================================================
 Run against the SAME MSSQL instance the app uses (see .env / Database.cs).
 Applies to change: daily-demand-variance-threshold (verify phase).

 Test parts use the THR-* prefix to avoid KanbanParser.TrimStart('P') stripping
 a leading 'P' from the part field. Kanban format: LOOP|THR-A|Q40|SKAN-TA1

 STEP 0 (cleanup) -> STEP 1 (daily_demand) -> STEP 2 (transactions) ->
 STEP 4 (dashboard baseline demand) are safe to run together.
 STEP 5 (cleanup) should be run AFTER the UI tests.
================================================================================
*/

SET NOCOUNT ON;
GO

/* ==========================================================================
   STEP 0 - Clean previous test rows (idempotent)
   ========================================================================== */
DELETE FROM dbo.transactions WHERE data_matrix LIKE 'THR-%';
DELETE FROM dbo.daily_demand  WHERE part_no LIKE 'THR-%';
GO

/* ==========================================================================
   STEP 1 - Seed daily_demand (use the CURRENT ISO week's Monday;
            2026-09-14 = ISO W38 -> business label W39)
   Columns mirror MssqlDailyDemandRepository:
   production_date, shift, model, part_no, quantity, scrapped, imported_at
   ========================================================================== */
INSERT INTO dbo.daily_demand (production_date, shift, model, part_no, quantity, scrapped, imported_at) VALUES
('2026-09-14', 1, 'PU-BODY', 'THR-A', 100, 0, SYSDATETIME()),
('2026-09-14', 1, 'PU-BODY', 'THR-B',  50, 0, SYSDATETIME()),
('2026-09-14', 1, 'PU-BODY', 'THR-C',  30, 0, SYSDATETIME()),
('2026-09-14', 1, 'PU-BODY', 'THR-D',  50, 0, SYSDATETIME());
GO

/* ==========================================================================
   STEP 2 - Seed transactions (column list mirrors seed_dummy_transactions.sql)
   ========================================================================== */
INSERT INTO dbo.transactions
(data_matrix, serial_no, model, part_no, quantity, kanban_no, operator_id, line_no, lane_no, trolley_no, station, status, created_at, updated_at, withdrawn_at, forpickup_at, received_at, consumed_at, is_suspected_nc, is_nc_confirmed, is_nc_rejected, nc_quantity)
VALUES
-- A: delivered 40 in-week -> variance 100-(40+0) = -60 -> ALLOWED
('THR-A-RCV-001','SRL-TA1','PU-BODY','THR-A',40,'KAN-TA1','OP-001','LINE-1','LN-A1','TR-TA1','ST001','Received',SYSDATETIME(),SYSDATETIME(),NULL,NULL,SYSDATETIME(),NULL,0,0,0,0),
-- B: delivered 50 in-week -> variance 50-(50+0) = 0 -> BLOCKED
('THR-B-RCV-001','SRL-TB1','PU-BODY','THR-B',50,'KAN-TB1','OP-001','LINE-1','LN-B1','TR-TB1','ST001','Received',SYSDATETIME(),SYSDATETIME(),NULL,NULL,SYSDATETIME(),NULL,0,0,0,0),
-- C: delivered 60 in-week, scrapped 10 -> variance 60-(30+10) = +20 -> BLOCKED
('THR-C-RCV-001','SRL-TC1','PU-BODY','THR-C',60,'KAN-TC1','OP-001','LINE-1','LN-C1','TR-TC1','ST001','Received',SYSDATETIME(),SYSDATETIME(),NULL,NULL,SYSDATETIME(),NULL,0,0,0,0),
('THR-C-SCR-001','SRL-TC2','PU-BODY','THR-C',10,'KAN-TC1','OP-001','LINE-1','LN-C2','TR-TC2','ST001','Scrapped',SYSDATETIME(),SYSDATETIME(),NULL,NULL,NULL,NULL,0,0,0,0),
-- D: delivered 50 OUT-of-week (received_at 8 days ago) -> excluded -> variance 0-(50+0) = -50 -> ALLOWED
('THR-D-RCV-001','SRL-TD1','PU-BODY','THR-D',50,'KAN-TD1','OP-001','LINE-1','LN-D1','TR-TD1','ST001','Received',DATEADD(DAY,-8,SYSDATETIME()),DATEADD(DAY,-8,SYSDATETIME()),NULL,NULL,DATEADD(DAY,-8,SYSDATETIME()),NULL,0,0,0,0);
GO

/* ==========================================================================
   STEP 3 - GATE MATRIX (Storage workstation, manual UI steps - NOT SQL)
   Run the storage flow for each part: scan kanban -> lane -> trolley -> cell
   -> operator -> APPLY -> PRINT -> verify-scan (scan the printed label or
   type the DataMatrix shown in the VERIFY step).

   | Part | Kanban to scan            | Expected result                                       |
   |------|---------------------------|-------------------------------------------------------|
   | A    | LOOP|THR-A|Q40|SKAN-TA1  | Toast "Storage Transaction Completed"; new Stored row |
   | B    | LOOP|THR-B|Q50|SKAN-TB1  | BLOCKED: "EXCEEDED THRESHOLD LIMIT"; no new row       |
   | C    | LOOP|THR-C|Q60|SKAN-TC1  | BLOCKED: "EXCEEDED THRESHOLD LIMIT"; no new row       |
   | D    | LOOP|THR-D|Q50|SKAN-TD1  | Toast "Storage Transaction Completed"; new Stored row |
   ========================================================================== */

/* ==========================================================================
   STEP 4 - Dashboard baseline demand (Planner login -> Dashboard tab)
   ========================================================================== */
INSERT INTO dbo.daily_demand (production_date, shift, model, part_no, quantity, scrapped, imported_at) VALUES
('2026-09-14', 1, 'PU-BODY', 'PU-BODY-001', 80, 0, SYSDATETIME()),
('2026-09-14', 1, 'PU-BODY', 'PU-BODY-004', 10, 0, SYSDATETIME()),
('2026-09-14', 1, 'PU-BODY', 'PU-BODY-005', 50, 0, SYSDATETIME()),
('2026-09-14', 1, 'PU-BODY', 'PU-BODY-007', 20, 0, SYSDATETIME());
GO

/* Expected in the Daily Demand & Delivery table (after seed_dummy_transactions.sql baseline):
   - PU-BODY-001: P2 Inventory = 100 (Stored 60+40 ONLY); Delivered 0; Variance -80.
   - PU-BODY-004: Delivered = 12 (Received in-week); Variance = 12-10-0 = +2.
   - PU-BODY-005: Delivered = 30 (Consumed in-week); Variance = -20.
   - PU-BODY-007: Delivered = 20; Scrapped = 8 (NC-confirmed); Variance = -8.
   - Every row's Date = W39 (2026-09-14 = ISO W38 -> +1), NOT W38 and NOT blank.
   - REQ-6: with THR-B (variance 0) or THR-C (+20) still seeded, run storage up to
     APPLY -> PRINT: the label must print and workflow reaches VERIFY. Only the
     final verification scan is blocked.
*/

/* ==========================================================================
   STEP 5 - Cleanup (run AFTER the UI tests)
   ========================================================================== */
-- DELETE FROM dbo.transactions WHERE data_matrix LIKE 'THR-%';
-- DELETE FROM dbo.daily_demand  WHERE part_no LIKE 'THR-%' OR part_no LIKE 'PU-BODY-%';
-- (Keep DUMMY- rows if you want the baseline grids populated.)
GO