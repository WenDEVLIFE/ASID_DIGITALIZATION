/*
================================================================================
 ASID - Manual Runtime Test Matrix: Daily Demand multi-week + NEW gate basis
================================================================================
 Change: daily-demand-multiweek-and-filters
 Gate basis under test: cumulative Stored-only P2 Inventory >= TOTAL all-week demand
 Run against the SAME MSSQL instance the app uses (see .env / Database.cs).

 SUPERSEDES test_threshold_matrix.sql (that one used the old variance basis and
 now produces flipped verdicts for THR-B and THR-C).

 Part numbers avoid a leading 'P' (KanbanParser.cs:13 does TrimStart('P')).

 Week mapping (verified):
   Excel Col A WW38 (ISO) -> production_date 2026-09-14 -> label "W39 (ISO W38)"
   Excel Col A WW39 (ISO) -> production_date 2026-09-21 -> label "W40 (ISO W39)"
   Excel Col A WW40 (ISO) -> production_date 2026-09-28 -> label "W41 (ISO W40)"

 ORDER OF EXECUTION:  T0 -> Seed -> T1..T6, T8 (UI) -> T7 (import, LAST) -> T9
 WARNING: the import's DeleteByWorkweek deletes the week for ALL parts, so run
          T7 last (or use weeks that do not collide with the seeded rows).
================================================================================
*/

SET NOCOUNT ON;
GO

/* ==========================================================================
   T0 - Cleanup (idempotent)
   ========================================================================== */
DELETE FROM dbo.transactions WHERE data_matrix LIKE 'MTR-%';
DELETE FROM dbo.daily_demand  WHERE part_no LIKE 'MTR-%';
GO

/* ==========================================================================
   SEED - run before the UI tests (T1-T6, T8)
   MTR-Z intentionally has NO rows (tests the 0 >= 0 parity case).
   ========================================================================== */
INSERT INTO dbo.daily_demand (production_date, shift, model, part_no, quantity, scrapped, imported_at) VALUES
 ('2026-09-14', 1, 'PU-BODY', 'MTR-A', 100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MTR-A', 100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MTR-A', 100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MTR-B', 100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MTR-B', 100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MTR-B', 100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MTR-C', 100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MTR-C', 100, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MTR-C', 100, 0, SYSDATETIME()),
 ('2026-09-14', 1, 'PU-BODY', 'MTR-MW', 100, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MTR-MW', 200, 0, SYSDATETIME()),
 ('2026-09-28', 1, 'PU-BODY', 'MTR-MW', 150, 0, SYSDATETIME()),
 ('2026-09-21', 1, 'PU-BODY', 'MTR-D', 100, 0, SYSDATETIME());
GO

INSERT INTO dbo.transactions
(data_matrix, serial_no, model, part_no, quantity, kanban_no, operator_id, line_no, lane_no, trolley_no, station, status, created_at, updated_at, withdrawn_at, forpickup_at, received_at, consumed_at, is_suspected_nc, is_nc_confirmed, is_nc_rejected, nc_quantity)
VALUES
-- MTR-A: Stored 50 counts; non-Stored 330 must NOT count (if counted -> 380 >= 300 would block)
('MTR-A-STORED','SRL-MA0','PU-BODY','MTR-A', 50,'KAN-MA1','OP-001','LINE-1','LN-A1','TR-A1','ST001','Stored','2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MTR-A-RCV','SRL-MA1','PU-BODY','MTR-A',120,'KAN-MA1','OP-001','LINE-1','LN-A2','TR-A2','ST001','Received','2026-09-16',NULL,NULL,NULL,'2026-09-22',NULL,0,0,0,0),
('MTR-A-CON','SRL-MA2','PU-BODY','MTR-A',130,'KAN-MA1','OP-001','LINE-1','LN-A3','TR-A3','ST001','Consumed','2026-09-16',NULL,NULL,NULL,NULL,'2026-09-29',0,0,0,0),
('MTR-A-SCR','SRL-MA3','PU-BODY','MTR-A', 80,'KAN-MA1','OP-001','LINE-1','LN-A4','TR-A4','ST001','Scrapped','2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
-- MTR-B: Stored 300 == total 300 -> BLOCK; delivered 0 -> variance -100 (old rule would ALLOW)
('MTR-B-STORED','SRL-MB0','PU-BODY','MTR-B',300,'KAN-MB1','OP-001','LINE-1','LN-B1','TR-B1','ST001','Stored','2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
-- MTR-C: Stored 299 < 300 -> boundary ALLOW
('MTR-C-STORED','SRL-MC0','PU-BODY','MTR-C',299,'KAN-MC1','OP-001','LINE-1','LN-C1','TR-C1','ST001','Stored','2026-09-16',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
-- MTR-MW: cumulative Stored 120 + per-week Delivered/Scrapped
('MTR-MW-STORED','SRL-MM0','PU-BODY','MTR-MW',120,'KAN-MM1','OP-001','LINE-1','LN-M1','TR-M1','ST001','Stored','2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MTR-MW-RCV39','SRL-MM1','PU-BODY','MTR-MW', 40,'KAN-MM1','OP-001','LINE-1','LN-M2','TR-M2','ST001','Received','2026-09-14',NULL,NULL,NULL,'2026-09-15',NULL,0,0,0,0),
('MTR-MW-CON39','SRL-MM2','PU-BODY','MTR-MW', 10,'KAN-MM1','OP-001','LINE-1','LN-M3','TR-M3','ST001','Consumed','2026-09-14',NULL,NULL,NULL,NULL,'2026-09-16',0,0,0,0),
('MTR-MW-SCR39','SRL-MM3','PU-BODY','MTR-MW',  5,'KAN-MM1','OP-001','LINE-1','LN-M4','TR-M4','ST001','Scrapped','2026-09-15',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MTR-MW-RCV40','SRL-MM4','PU-BODY','MTR-MW', 60,'KAN-MM1','OP-001','LINE-1','LN-M5','TR-M5','ST001','Received','2026-09-21',NULL,NULL,NULL,'2026-09-22',NULL,0,0,0,0),
('MTR-MW-CON40','SRL-MM5','PU-BODY','MTR-MW', 20,'KAN-MM1','OP-001','LINE-1','LN-M6','TR-M6','ST001','Consumed','2026-09-21',NULL,NULL,NULL,NULL,'2026-09-23',0,0,0,0),
('MTR-MW-SCR40','SRL-MM6','PU-BODY','MTR-MW', 10,'KAN-MM1','OP-001','LINE-1','LN-M7','TR-M7','ST001','Scrapped','2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0),
('MTR-MW-RCV41','SRL-MM7','PU-BODY','MTR-MW', 30,'KAN-MM1','OP-001','LINE-1','LN-M8','TR-M8','ST001','Received','2026-09-28',NULL,NULL,NULL,'2026-09-29',NULL,0,0,0,0),
-- MTR-MW UpdatedAt fallback: created_at NULL -> ScrapTimestamp uses updated_at (W41)
('MTR-MW-SCR41FB','SRL-MM8','PU-BODY','MTR-MW', 7,'KAN-MM1','OP-001','LINE-1','LN-M9','TR-M9','ST001','Scrapped',NULL,'2026-09-29',NULL,NULL,NULL,NULL,0,0,0,0),
-- MTR-D: Stored 150 vs demand 100 -> RED
('MTR-D-STORED','SRL-MD0','PU-BODY','MTR-D',150,'KAN-MD1','OP-001','LINE-1','LN-D1','TR-D1','ST001','Stored','2026-09-22',NULL,NULL,NULL,NULL,NULL,0,0,0,0);
GO

/* ==========================================================================
   T1 - GATE ALLOW: MTR-A (P2 50 < total demand 300)
   Kanban: LOOP|MTR-A|Q50|SKAN-MA1
   Full storage flow: kanban -> vacant lane -> trolley -> cell -> operator ->
   APPLY -> PRINT -> verify-scan.
   EXPECTED: completes ("Storage Transaction Completed") + new Stored row.
   Proves non-Stored units are excluded (330 excluded; else 380 >= 300 would block).
   ========================================================================== */

/* ==========================================================================
   T2 - GATE BLOCK: MTR-B (P2 300 >= total demand 300, variance -100)
   Kanban: LOOP|MTR-B|Q50|SKAN-MB1
   EXPECTED: centered pop-up "EXCEEDED THRESHOLD LIMIT"; NO new Stored row.
   Repeatable. The OLD variance rule would have ALLOWED this.
   ========================================================================== */

/* ==========================================================================
   T3 - GATE BOUNDARY ALLOW: MTR-C (P2 299 < total demand 300)
   Kanban: LOOP|MTR-C|Q10|SKAN-MC1
   EXPECTED: allowed. Run ONCE only - the stored Q pushes P2 to >= 300 and a
   second scan would block.
   ========================================================================== */

/* ==========================================================================
   T4 - ALL-WEEKS VIEW + COLOR (Dashboard tab)
   EXPECTED for MTR-MW - three rows, one per week, SAME cumulative P2:

     Week label      | Demand | P2  | Delivered | Scrapped | Variance
     W39 (ISO W38)   |  100   | 120 |    50     |    5     |  -55
     W40 (ISO W39)   |  200   | 120 |    80     |   10     | -130
     W41 (ISO W40)   |  150   | 120 |    30     |    7     | -127

   COLOR:
     MTR-MW  120/450 = 0.27  -> no color
     MTR-B   300/300 = 1.00  -> YELLOW
     MTR-D   150/100 = 1.50  -> RED
     MTR-Z   demand 0        -> no color
   ========================================================================== */

/* ==========================================================================
   T5 - WEEK-FILTER SCOPING
   Select each week in the Week combo:
     W40 (ISO W39) -> Demand 200, Delivered 80, Scrapped 10, Variance -130,
                      P2 still 120
     W39 (ISO W38) -> Demand 100, Delivered 50, Scrapped  5, Variance  -55,
                      P2 still 120
     W41 (ISO W40) -> Demand 150, Delivered 30, Scrapped  7 (created_at NULL
                      row counted via updated_at), Variance -127, P2 still 120
     All weeks     -> all three rows
   T5b (optional): set MTR-MW-SCR41FB.updated_at to a W39 date and confirm the
   scrap moves from W41 to W39.
   ========================================================================== */

/* ==========================================================================
   T6 - PERSISTENCE ACROSS TWO 5s REFRESH TICKS
   1. Select week W40 (ISO W39).
   2. Type "MTR" in Model (or "MTR-MW" in Part No), type "MW" in Search.
   3. Click the Week sort button; drag a column border to a non-default width;
      scroll the grid down.
   4. Wait >= 10 seconds (two DashboardController 5s ticks).
   EXPECTED: combo selection, filter text, search text, sort order, column
   width and scroll offset all unchanged; only row values refresh.
   ========================================================================== */

/* ==========================================================================
   T8 - ZERO-DEMAND PARITY
   Kanban: LOOP|MTR-Z|Q10|SKAN-MZ1 (no seeded demand, no transactions)
   EXPECTED: BLOCKED with "EXCEEDED THRESHOLD LIMIT" (0 >= 0), and no color on
   any zero-demand row.
   ========================================================================== */

/* ==========================================================================
   T7 - IMPORT APPEND (RUN LAST - see W3 warning)
   Minimal workbook: headers rows 2-4, data from row 5, blank Col A carries the
   previous week forward.

     Row | A Work Week | B Serial Production | D PU Body PN | E Rev. 0
      2  | (blank)     | Serial Production   | PU Body PN   | Rev. 0
      5  | 38          | PU-BODY             | MTR-IMP      | 100
      6  | (blank)     | PU-BODY             | MTR-IMP      |  60

   1. Import File A (WW38), then File B (WW39, part MTR-IMP, qty 200).
   2. Verify with the SELECT below -> two rows: 2026-09-14 (160) and
      2026-09-21 (200). The W39 row survived.
   3. Re-import File B -> still exactly two rows (2026-09-21 replaced, not
      duplicated).
   ========================================================================== */
-- SELECT production_date, part_no, SUM(quantity) AS demand, COUNT(*) AS rows
-- FROM dbo.daily_demand WHERE part_no = 'MTR-IMP'
-- GROUP BY production_date, part_no ORDER BY production_date;

/* ==========================================================================
   T9 - Cleanup (run after all tests)
   ========================================================================== */
-- DELETE FROM dbo.transactions WHERE data_matrix LIKE 'MTR-%';
-- DELETE FROM dbo.daily_demand  WHERE part_no LIKE 'MTR-%';
GO