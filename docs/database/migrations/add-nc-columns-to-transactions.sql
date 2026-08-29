/*
===============================================================================
 Migration: Add NC confirmation columns to transactions table
===============================================================================
 Purpose : Adds is_nc_confirmed, is_nc_rejected, nc_quantity columns.
 Run     : Execute against your live PostgreSQL database.
===============================================================================
*/

ALTER TABLE transactions
  ADD COLUMN IF NOT EXISTS is_nc_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
  ADD COLUMN IF NOT EXISTS is_nc_rejected  BOOLEAN NOT NULL DEFAULT FALSE,
  ADD COLUMN IF NOT EXISTS nc_quantity     INTEGER NOT NULL DEFAULT 0;

-- Verify
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'transactions'
  AND column_name IN ('is_suspected_nc', 'is_nc_confirmed', 'is_nc_rejected', 'nc_quantity')
ORDER BY column_name;
