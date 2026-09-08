-- Check how many rows will be deleted
SELECT COUNT(*) AS TotalRows FROM dbo.transactions;

-- Preview some data
SELECT TOP 10 * FROM dbo.transactions ORDER BY created_at DESC;

-- Then delete
DELETE FROM dbo.transactions;
GO