-- ============================================
-- LESSON 13: Stored Procedures
-- Step 1: Create the stored procedures in SQL Server
-- Run this file in SSMS before running Program.cs
-- ============================================

USE ChelloApp;
GO

-- ============================================
-- Get all students, ordered by score
-- ============================================

CREATE OR ALTER PROCEDURE sp_GetAllStudents
AS
BEGIN
    SELECT Id, FirstName, LastName, Email, Score
    FROM Students
    ORDER BY Score DESC;
END
GO

-- ============================================
-- Get one student by ID
-- ============================================

CREATE OR ALTER PROCEDURE sp_GetStudentById
    @StudentId INT
AS
BEGIN
    SELECT Id, FirstName, LastName, Email, Score
    FROM Students
    WHERE Id = @StudentId;
END
GO

-- ============================================
-- Add a new student
-- @NewId is an OUTPUT parameter — SQL writes
-- the new auto-generated ID into it so C# can read it back
-- ============================================

CREATE OR ALTER PROCEDURE sp_AddStudent
    @FirstName  NVARCHAR(50),
    @LastName   NVARCHAR(50),
    @Email      NVARCHAR(100),
    @Score      INT,
    @NewId      INT OUTPUT
AS
BEGIN
    INSERT INTO Students (FirstName, LastName, Email, Score)
    VALUES (@FirstName, @LastName, @Email, @Score);

    SET @NewId = SCOPE_IDENTITY();
END
GO

-- ============================================
-- Update a student's score
-- Returns the number of rows changed via SELECT
-- ============================================

CREATE OR ALTER PROCEDURE sp_UpdateScore
    @StudentId  INT,
    @NewScore   INT
AS
BEGIN
    UPDATE Students
    SET Score = @NewScore
    WHERE Id = @StudentId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT 'All stored procedures created successfully!';
