-- ============================================
-- LESSON 9: SQL Basics
-- Step 4: Read Data (SELECT)
-- ============================================

USE ChelloApp;
GO

-- Get ALL students, ALL columns
SELECT * FROM Students;

-- Get specific columns only
SELECT FirstName, LastName, Score FROM Students;

-- Filter - students who scored 70 or above
SELECT * FROM Students
WHERE Score >= 70;

-- Sort by score highest to lowest
SELECT * FROM Students
ORDER BY Score DESC;

-- Sort by score lowest to highest
SELECT * FROM Students
ORDER BY Score ASC;

-- Filter AND sort together
SELECT FirstName, LastName, Score
FROM Students
WHERE Score >= 70
ORDER BY Score DESC;

-- Count how many students
SELECT COUNT(*) AS TotalStudents FROM Students;

-- Get the highest score
SELECT MAX(Score) AS HighestScore FROM Students;

-- Get the lowest score
SELECT MIN(Score) AS LowestScore FROM Students;

-- Get the average score
SELECT AVG(Score) AS AverageScore FROM Students;
