-- =============================================
-- Lane Management Table
-- Stores lane configuration, occupancy, and
-- status for all 100 lanes (A-01 to A-50,
-- B-01 to B-50)
-- =============================================

IF OBJECT_ID('dbo.lane_management', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.lane_management
    (
        id                INT IDENTITY(1,1) PRIMARY KEY,
        lane_no           NVARCHAR(20) NOT NULL UNIQUE,
        part_no           NVARCHAR(100) NOT NULL DEFAULT 'Not Assigned',
        max_qty_stored    INT NOT NULL DEFAULT 100,
        actual_stored_qty INT NOT NULL DEFAULT 0,
        withdrawn_qty     INT NOT NULL DEFAULT 0,
        lane_status       NVARCHAR(50) NOT NULL DEFAULT 'Not Assigned',
        color_status      NVARCHAR(20) NOT NULL DEFAULT 'Gray',
        created_at        DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        updated_at        DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END;
GO


-- =============================================
-- Seed sample data
-- Only inserts if the table is empty
-- =============================================

IF NOT EXISTS (SELECT 1 FROM dbo.lane_management)
BEGIN

    INSERT INTO dbo.lane_management
    (
        lane_no,
        part_no,
        max_qty_stored,
        actual_stored_qty,
        withdrawn_qty,
        lane_status,
        color_status
    )
    SELECT
        lane_no,
        part_no,
        max_qty_stored,
        actual_stored_qty,
        withdrawn_qty,
        lane_status,
        color_status
    FROM
    (
        VALUES
        ('A-01', '647187100F', 4, 8, 4, 'Occupied', 'Red'),
        ('A-02', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-03', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-04', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-05', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-06', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-07', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-08', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-09', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-10', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-11', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-12', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-13', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-14', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-15', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-16', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-17', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-18', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-19', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-20', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-21', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-22', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-23', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-24', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-25', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-26', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-27', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-28', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-29', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-30', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-31', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-32', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-33', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-34', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-35', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-36', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-37', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-38', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-39', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-40', 'Not Assigned', 100, 50, 45, 'Occupied', 'Green'),
        ('A-41', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-42', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-43', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-44', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-45', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-46', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-47', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-48', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-49', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('A-50', 'Not Assigned', 100, 100, 0, 'Full', 'Red'),

        ('B-01', '647187000A', 4, 2, 2, 'Vacant', 'Green'),
        ('B-02', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-03', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-04', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-05', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-06', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-07', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-08', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-09', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-10', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-11', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-12', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-13', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-14', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-15', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-16', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-17', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-18', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-19', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-20', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-21', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-22', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-23', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-24', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-25', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-26', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-27', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-28', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-29', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-30', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-31', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-32', '640578600E', 100, 2, 0, 'Vacant', 'Green'),
        ('B-33', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-34', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-35', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-36', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-37', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-38', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-39', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-40', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-41', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-42', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-43', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-44', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-45', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-46', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-47', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-48', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-49', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray'),
        ('B-50', 'Not Assigned', 100, 0, 0, 'Not Assigned', 'Gray')
    ) AS v
    (
        lane_no,
        part_no,
        max_qty_stored,
        actual_stored_qty,
        withdrawn_qty,
        lane_status,
        color_status
    );

END;
GO