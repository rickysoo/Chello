-- ============================================================
-- Lesson 34: User Registration — Sign up and save to DB
-- Step 1: Create the Users table in ChelloApp database
-- ============================================================

-- Make sure we are using the right database
USE ChelloApp;
GO

-- ============================================================
-- DROP TABLE IF EXISTS (for easy re-running during development)
-- ============================================================
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

-- ============================================================
-- CREATE TABLE Users
-- This table stores everyone who signs up on our app
-- ============================================================
CREATE TABLE Users (

    -- Id: a unique number for each user, auto-incremented by SQL Server
    -- IDENTITY(1,1) means: start at 1, go up by 1 each time
    -- PRIMARY KEY means: this column uniquely identifies each row
    Id INT IDENTITY(1,1) PRIMARY KEY,

    -- Username: the display name chosen by the user
    -- NVARCHAR supports international characters (N = Unicode)
    -- 50 characters max, cannot be empty (NOT NULL)
    -- UNIQUE means: no two users can have the same username
    Username NVARCHAR(50) NOT NULL UNIQUE,

    -- Email: the user's email address for login and contact
    -- 100 characters max, cannot be empty
    -- UNIQUE means: each email can only register once
    Email NVARCHAR(100) NOT NULL UNIQUE,

    -- PasswordHash: we NEVER store plain passwords
    -- We store a "hash" — a one-way scrambled version of the password
    -- 256 characters is enough for SHA-256 hash output (64 hex chars)
    -- In production, use BCrypt which produces longer hashes (~60 chars)
    PasswordHash NVARCHAR(256) NOT NULL,

    -- CreatedAt: the date and time when the user registered
    -- DEFAULT GETDATE() means SQL Server fills this in automatically
    -- You do not need to provide this value when inserting a new user
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()

);
GO

-- ============================================================
-- Add an index on Email for faster lookups during login
-- When a user logs in, we search by email — an index speeds this up
-- ============================================================
CREATE INDEX IX_Users_Email ON Users(Email);
GO

-- ============================================================
-- Verify the table was created correctly
-- ============================================================
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Users table created successfully!';
GO
