/*
================================================================================
 ASID - Manual Runtime Test Matrix (REV 2): current-week gate + per-row color
================================================================================
 Change: daily-demand-multiweek-and-filters  (REV 2 correction)

 GATE BASIS : cumulative Stored-only P2 Inventory >= CURRENT week's demand
              (Variance is DISPLAY-ONLY and never gates)
 COLOR BASIS: Daily Demand rows  -> P2 / that row's week demand (90-100% yellow, >=101% red)
              PU-Body Inventory -> P2 / CURRENT-week demand

 VALIDITY WINDOW: anchored to today = 2026-09-21 (ISO W39, Mon 09-21..Sun 09-27).
   Current week Monday = 2026-09-21 -> dashboard label "W40 (ISO W39)".
   If run OUTSIDE that window, re-anchor all three production_date values to the
   current week's Monday before seeding.

 SUPERSEDES: the REV 1 (all-week) content of this same file, and
   test_threshold_matrix.sql (variance basis) which is fully obsolete.

 Part numbers avoid a leading 'P' (KanbanParser.TrimStart('P')).
 ORDER: T0 -> T-SEED -> T1..T10 (UI) -> T11 (import, LAST) -> T12 (cleanup)
================================================================================
*/

SET NOCOUNT ON;
GO

/* ============================================================================
   T0 - Cleanup (idempotent)
   ============================================================================ */
DELETE FROM dbo.transactions WHERE data_matrix LIKE 'MR2-%';
DELETE FROM dbo.daily_demand  WHERE part_no     LIKE 'MR2-%';
GO

/* ============================================================================
   T-SEED - demand + transactions (run before the UI tests)
   MR2-Z has NO demand rows -> exercises the 0 >= 0 parity block.
   ============================================================================ */
INSERT INTO dbo.daily_demand (production_date, shift, model, part_no, quantity, scrapped, imported_at) VALUES
 ('2026-09-14', 1, 'PU-BODY', 'MR2-ALLOW',    100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-ALLOW',    100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MR2-ALLOW',    100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MR2-EQ',       100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-EQ',       100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MR2-EQ',       100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MR2-ABOVE',    100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-ABOVE',    100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MR2-ABOVE',    100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MR2-FLIP',     100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-FLIP',     100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MR2-FLIP',     100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MR2-VARINERT', 100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-VARINERT', 100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MR2-VARINERT', 100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MR2-COLOR',    100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-COLOR',    200, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MR2-COLOR',    150, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-YEL',      100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-INVRED',   100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MR2-ZERO',       0, 0, SYSDATETIME());
GO

INSERT INTO dbo.transactions
(data_matrix, serial_no, model, part_no, quantity, kanban_no, operator_id, line_no, lane_no, trolley_no, station, status, created_at, updated_at, withdrawn_at, forpickup_at, received_at, consumed_at, is_suspected_nc, is_nc_confirmed, is_nc_rejected, nc_quantity)
VALUES
-- MR2-ALLOW: Stored 50 counts; non-Stored 330 must NOT count (else 380 >= 100 would BLOCK)
('MR2-ALLOW-STORED','SRL-RA0','PU-BODY','MR2-ALLOW',  50,'KAN-RA1','OP-001','LINE-1','LN-A1','TR-A1','ST001','Stored'  ,'2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MR2-ALLOW-RCV'   ,'SRL-RA1','PU-BODY','MR2-ALLOW', 120,'KAN-RA1','OP-001','LINE-1','LN-A2','TR-A2','ST001','Received','2026-09-16',NULL,NULL,NULL,'2026-09-22',NULL,0,0,0,0),
('MR2-ALLOW-CON'   ,'SRL-RA2','PU-BODY','MR2-ALLOW', 130,'KAN-RA1','OP-001','LINE-1','LN-A3','TR-A3','ST001','Consumed','2026-09-16',NULL,NULL,NULL,NULL,'2026-09-29',0,0,0,0),
('MR2-ALLOW-SCR'   ,'SRL-RA3','PU-BODY','MR2-ALLOW',  80,'KAN-RA1','OP-001','LINE-1','LN-A4','TR-A4','ST001','Scrapped','2026-09-15',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
-- MR2-EQ: Stored 100 == current-week demand 100 -> BLOCK
('MR2-EQ-STORED'   ,'SRL-RE0','PU-BODY','MR2-EQ'   , 100,'KAN-RE1','OP-001','LINE-1','LN-E1','TR-E1','ST001','Stored'  ,'2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
-- MR2-ABOVE: Stored 150 >= current-week demand 100 -> BLOCK
('MR2-ABOVE-STORED','SRL-RB0','PU-BODY','MR2-ABOVE', 150,'KAN-RB1','OP-001','LINE-1','LN-B1','TR-B1','ST001','Stored'  ,'2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
-- MR2-FLIP: Stored 299 < all-week 300 (old rule ALLOWED) but >= current-week 100 (REV 2 BLOCKS)
('MR2-FLIP-STORED','SRL-RF0','PU-BODY','MR2-FLIP'  , 299,'KAN-RF1','OP-001','LINE-1','LN-F1','TR-F1','ST001','Stored'  ,'2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
-- MR2-VARINERT: P2 50 and variance +50 -> ALLOWED (proves variance does not gate)
('MR2-VARINERT-STORED','SRL-RV0','PU-BODY','MR2-VARINERT', 50,'KAN-RV1','OP-001','LINE-1','LN-V1','TR-V1','ST001','Stored'  ,'2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MR2-VARINERT-RCV'   ,'SRL-RV1','PU-BODY','MR2-VARINERT',150,'KAN-RV1','OP-001','LINE-1','LN-V2','TR-V2','ST001','Received','2026-09-21',NULL,NULL,NULL,'2026-09-22',NULL,0,0,0,0),
-- MR2-COLOR: cumulative Stored 120 + per-week Delivered/Scrapped (per-row color test)
('MR2-COLOR-STORED','SRL-RC0','PU-BODY','MR2-COLOR', 120,'KAN-RC1','OP-001','LINE-1','LN-C1','TR-C1','ST001','Stored'  ,'2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MR2-COLOR-RCV38' ,'SRL-RC1','PU-BODY','MR2-COLOR',  40,'KAN-RC1','OP-001','LINE-1','LN-C2','TR-C2','ST001','Received','2026-09-14',NULL,NULL,NULL,'2026-09-15',NULL,0,0,0,0),
('MR2-COLOR-CON38' ,'SRL-RC2','PU-BODY','MR2-COLOR',  10,'KAN-RC1','OP-001','LINE-1','LN-C3','TR-C3','ST001','Consumed','2026-09-14',NULL,NULL,NULL,NULL,'2026-09-16',0,0,0,0),
('MR2-COLOR-SCR38' ,'SRL-RC3','PU-BODY','MR2-COLOR',   5,'KAN-RC1','OP-001','LINE-1','LN-C4','TR-C4','ST001','Scrapped','2026-09-15',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MR2-COLOR-RCV39' ,'SRL-RC4','PU-BODY','MR2-COLOR',  60,'KAN-RC1','OP-001','LINE-1','LN-C5','TR-C5','ST001','Received','2026-09-21',NULL,NULL,NULL,'2026-09-22',NULL,0,0,0,0),
('MR2-COLOR-CON39' ,'SRL-RC5','PU-BODY','MR2-COLOR',  20,'KAN-RC1','OP-001','LINE-1','LN-C6','TR-C6','ST001','Consumed','2026-09-21',NULL,NULL,NULL,NULL,'2026-09-23',0,0,0,0),
('MR2-COLOR-SCR39' ,'SRL-RC6','PU-BODY','MR2-COLOR',  10,'KAN-RC1','OP-001','LINE-1','LN-C7','TR-C7','ST001','Scrapped','2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MR2-COLOR-RCV40' ,'SRL-RC7','PU-BODY','MR2-COLOR',  30,'KAN-RC1','OP-001','LINE-1','LN-C8','TR-C8','ST001','Received','2026-09-28',NULL,NULL,NULL,'2026-09-29',NULL,0,0,0,0),
('MR2-COLOR-SCR40FB','SRL-RC8','PU-BODY','MR2-COLOR',  7,'KAN-RC1','OP-001','LINE-1','LN-C9','TR-C9','ST001','Scrapped',NULL,'2026-09-29',NULL,NULL,NULL,NULL,0,0,0,0),
-- MR2-YEL 95/100 -> YELLOW ; MR2-INVRED 150/100 -> RED ; MR2-ZERO demand 0 -> no color
('MR2-YEL-STORED'  ,'SRL-RY0','PU-BODY','MR2-YEL'  ,  95,'KAN-RY1','OP-001','LINE-1','LN-Y1','TR-Y1','ST001','Stored'  ,'2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MR2-INVRED-STORED','SRL-RD0','PU-BODY','MR2-INVRED',150,'KAN-RD1','OP-001','LINE-1','LN-D1','TR-D1','ST001','Stored'  ,'2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MR2-ZERO-STORED' ,'SRL-RZ0','PU-BODY','MR2-ZERO' , 100,'KAN-RZ1','OP-001','LINE-1','LN-Z1','TR-Z1','ST001','Stored'  ,'2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0);
GO

/* ============================================================================
   T1..T6  GATE TESTS - Storage workstation, full flow each time:
   kanban -> vacant lane -> trolley -> line/cell -> operator -> APPLY -> PRINT
   -> verification scan.

   T1 MR2-ALLOW     kanban LOOP|MR2-ALLOW|Q50|SKAN-RA1     -> ALLOWED (P2 50 < 100)
        Extra proof: 330 non-Stored units did NOT count (else 380 >= 100).
        Current-week variance = 120-(100+0) = +20 (>= 0) yet ALLOWED = variance inert.
   T2 MR2-EQ        kanban LOOP|MR2-EQ|Q50|SKAN-RE1        -> BLOCKED (100 >= 100 boundary)
   T3 MR2-ABOVE     kanban LOOP|MR2-ABOVE|Q10|SKAN-RB1     -> BLOCKED (150 >= 100)
   T4 MR2-FLIP      kanban LOOP|MR2-FLIP|Q50|SKAN-RF1      -> BLOCKED  <-- REV 2 FLIP
        All-week total = 300, so the OLD rule (299 >= 300) ALLOWED this scan;
        the CURRENT-week rule (299 >= 100) BLOCKS it.
   T5 MR2-VARINERT  kanban LOOP|MR2-VARINERT|Q50|SKAN-RV1  -> ALLOWED  <-- variance inert
        Variance = 150 - (100 + 0) = +50 (>= 0): the OLD variance rule would
        have BLOCKED; REV 2 requires ALLOWED.
   T6 MR2-Z         kanban LOOP|MR2-Z|Q10|SKAN-RZ1         -> BLOCKED (0 >= 0, no rows)

   Every BLOCK must show the centered pop-up "EXCEEDED THRESHOLD LIMIT"
   and must NOT commit a new Stored row.
   ============================================================================ */

/* ============================================================================
   T7  Daily Demand tab: all-weeks view + PER-ROW color
   Filter Part No = MR2-COLOR, Week = All weeks. Expect 3 rows, same P2 = 120:

   Production Week        Demand  P2   Delivered  Scrapped  Variance   ratio   Cell
   W39 (ISO W38)           100    120      50         5       -55      1.20    RED
   W40 (ISO W39) CURRENT   200    120      80        10      -130      0.60    none
   W41 (ISO W40)           150    120      30         7      -127      0.80    none

   The W39 row turning RED is the visible REV 2 change: the old all-week
   denominator (450) gave 120/450 = 0.27 -> no color anywhere.

   Same per-row rule on other parts:
     MR2-YEL    95/100 = 0.95 -> YELLOW
     MR2-INVRED 150/100 = 1.50 -> RED
     MR2-EQ     100/100 = 1.00 -> YELLOW
     MR2-ZERO   demand 0       -> none
   ============================================================================ */

/* ============================================================================
   T8  Week-filter scoping (P2 must stay cumulative in every selection)
   Keep the MR2-COLOR filter and select each week:
     W40 (ISO W39) -> Demand 200, Delivered 80, Scrapped 10, Variance -130, P2 120
     W39 (ISO W38) -> Demand 100, Delivered 50, Scrapped  5, Variance  -55, P2 120
     W41 (ISO W40) -> Demand 150, Delivered 30, Scrapped  7, Variance -127, P2 120
     All weeks     -> all three rows

   T8b (optional) fallback isolation - the created_at-NULL scrap row uses updated_at:
     UPDATE dbo.transactions SET updated_at = '2026-09-16'
      WHERE data_matrix = 'MR2-COLOR-SCR40FB';
   ============================================================================ */

/* ============================================================================
   T9  Persistence across two 5s refresh ticks
   1. Select week W40 (ISO W39).
   2. Type MR2 in Model (or MR2-COLOR in Part No) and COLOR in Search.
   3. Click the Week sort button; drag a column border to a non-default width;
      scroll the grid down.
   4. Wait >= 10 seconds (two DashboardController 5s ticks).
   EXPECTED: combo selection, filter texts, search text, sort order, column
   width and scroll offset all UNCHANGED; only values refresh.
   ============================================================================ */

/* ============================================================================
   T10 PU-Body Inventory tab: CURRENT-week color (different basis)
   Part        P2 Supermarket   current-week demand   ratio   Cell
   MR2-COLOR        120                200            0.60    none
   MR2-INVRED       150                100            1.50    RED
   MR2-YEL           95                100            0.95    YELLOW
   MR2-EQ           100                100            1.00    YELLOW
   MR2-ZERO         100                  0              -     none

   KEY CONTRAST: MR2-COLOR is RED on its W39 Daily Demand row (row demand 100)
   but has NO color in the inventory table (current-week demand 200) - one part,
   two different ratios, exactly the REV 2 split.
   ============================================================================ */

/* ============================================================================
   T11 Import append - RUN LAST
   WARNING: DeleteByWorkweek removes that whole week for ALL parts, so importing
   WW38/WW39 will delete the seeded MR2 rows dated 2026-09-14 / 2026-09-21 and
   change the earlier expectations. Run this last, then clean up.

   Minimal workbook (headers rows 2-4; data from row 5; blank Col A carries the
   previous week forward):
     Row 2: (blank)  | Serial Production | PU Body PN | Rev. 0
     Row 5: 38       | PU-BODY           | MR2-IMP    | 100
     Row 6: (blank)  | PU-BODY           | MR2-IMP    |  60

   1. Import File A (WW38), then File B (WW39, part MR2-IMP, qty 200).
   2. Expected: exactly two rows - 2026-09-14 (160) and 2026-09-21 (200).
      The WW38 row survived File B.
   3. Re-import File B -> still two rows (2026-09-21 replaced, not duplicated).
   4. The multi-week preview label shows "WW 38, WW 39" (display-only check).
   ============================================================================ */
-- SELECT production_date, part_no, SUM(quantity) AS demand, COUNT(*) AS rows
-- FROM dbo.daily_demand WHERE part_no = 'MR2-IMP'
-- GROUP BY production_date, part_no ORDER BY production_date;

/* ============================================================================
   T12 Cleanup (run after all tests)
   ============================================================================ */
-- DELETE FROM dbo.transactions WHERE data_matrix LIKE 'MR2-%';
-- DELETE FROM dbo.daily_demand  WHERE part_no     LIKE 'MR2-%';
GO