-- ============================================
-- LESSON 9: SQL Basics
-- Step 3: Insert Data (CREATE)
-- ============================================

USE ChelloApp;
GO

-- Insert students one by one
INSERT INTO Students (FirstName, LastName, Email, Score)
VALUES ('Ricky', 'Soo', 'ricky@email.com', 95);

INSERT INTO Students (FirstName, LastName, Email, Score)
VALUES ('Alice', 'Wong', 'alice@email.com', 88);

INSERT INTO Students (FirstName, LastName, Email, Score)
VALUES ('Bob', 'Tan', 'bob@email.com', 72);

INSERT INTO Students (FirstName, LastName, Email, Score)
VALUES ('Charlie', 'Lim', 'charlie@email.com', 60);

INSERT INTO Students (FirstName, LastName, Email, Score)
VALUES ('Diana', 'Lee', 'diana@email.com', 45);

PRINT '5 students inserted successfully!';
