-- ============================================
-- LESSON 9: SQL Basics
-- Step 2: Create a Table
-- ============================================

USE ChelloApp;
GO

-- Create a Students table
CREATE TABLE Students (
    Id          INT             PRIMARY KEY IDENTITY(1,1),  -- auto number, unique ID
    FirstName   NVARCHAR(50)    NOT NULL,                   -- text, required
    LastName    NVARCHAR(50)    NOT NULL,                   -- text, required
    Email       NVARCHAR(100)   NOT NULL,                   -- text, required
    Score       INT             NOT NULL DEFAULT 0,         -- number, defaults to 0
    CreatedAt   DATETIME        NOT NULL DEFAULT GETDATE()  -- date, defaults to now
);
GO

PRINT 'Table Students created successfully!';
