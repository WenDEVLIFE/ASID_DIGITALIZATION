/*
===============================================================================
 DATA FIX - remove the wrongly retained 'P' field TAG from transactions.part_no
===============================================================================

 CONTEXT
   The PU-Body kanban payload is pipe-delimited and EVERY field carries a
   single-letter tag prefix:
       field 1 -> 'P' + part number
       field 2 -> 'Q' + quantity
       field 3 -> 'S' + kanban number
   A regression temporarily stopped stripping the 'P' tag, so rows scanned
   during that window were stored WITH the tag (e.g. "P647187100F" instead of
   "647187100F") - and because the Model lookup compared raw values, those rows
   also ended up with a BLANK Model.

   New scans are already fixed in code (KanbanParser). This script cleans the
   rows that were already stored.

 SCOPE / SAFETY
   - ONLY rows whose 'P'-stripped part number MATCHES a row in daily_demand are
     touched. A legitimate part number that really starts with 'P' (and has no
     matching plan row without it) is therefore left alone.
   - Idempotent: after the fix, no row satisfies the LIKE 'P%' + join condition.
   - Run the PRE-CHECK first and keep the output as your backup reference.
   - The UPDATE is inside an explicit transaction: inspect, then COMMIT or
     ROLLBACK.

 NOTE
   updated_at is deliberately NOT modified - it feeds the Scrapped week
   attribution fallback, so a data fix should not disturb it.
===============================================================================
*/

/* ===================== 1. PRE-CHECK (inspect before changing) ============= */
SELECT
    t.id,
    t.part_no                       AS old_part_no,
    STUFF(t.part_no, 1, 1, '')      AS new_part_no,
    t.model                         AS old_model,
    d.model                         AS plan_model,
    t.status,
    t.created_at
FROM dbo.transactions t
CROSS APPLY (
    SELECT TOP 1 d.model
    FROM dbo.daily_demand d
    WHERE d.part_no = STUFF(t.part_no, 1, 1, '')
    ORDER BY d.production_date DESC
) d
WHERE t.part_no LIKE 'P%'
ORDER BY t.created_at;
GO


/* ===================== 2. FIX (transactional) ============================= */
BEGIN TRAN;

UPDATE t
SET t.part_no = STUFF(t.part_no, 1, 1, ''),
    t.model   = COALESCE(NULLIF(t.model, ''), d.model)
FROM dbo.transactions t
CROSS APPLY (
    SELECT TOP 1 d.model
    FROM dbo.daily_demand d
    WHERE d.part_no = STUFF(t.part_no, 1, 1, '')
    ORDER BY d.production_date DESC
) d
WHERE t.part_no LIKE 'P%';

PRINT CONCAT('Rows updated: ', @@ROWCOUNT);


/* ===================== 3. POST-CHECK ===================================== */
-- Rows still holding an unmatched 'P' part number (these were LEFT ALONE on
-- purpose - verify they are genuinely P-prefixed part numbers, not leftovers):
SELECT COUNT(*) AS remaining_prefixed_rows
FROM dbo.transactions t
WHERE t.part_no LIKE 'P%'
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.daily_demand d
      WHERE d.part_no = STUFF(t.part_no, 1, 1, '')
  );

-- The fixed rows:
SELECT TOP 30 id, part_no, model, status, created_at
FROM dbo.transactions
WHERE part_no NOT LIKE 'P%'
ORDER BY created_at DESC;


/* ===================== 4. DECIDE ======================================== */
-- If everything looks correct:
-- COMMIT TRAN;
-- Otherwise:
-- ROLLBACK TRAN;
GO
