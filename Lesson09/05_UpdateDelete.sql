-- ============================================
-- LESSON 9: SQL Basics
-- Step 5: Update and Delete
-- ============================================

USE ChelloApp;
GO

-- UPDATE - change Bob's score to 80
UPDATE Students
SET Score = 80
WHERE FirstName = 'Bob' AND LastName = 'Tan';

-- UPDATE - change email for a specific ID
UPDATE Students
SET Email = 'ricky.soo@email.com'
WHERE Id = 1;

-- See the changes
SELECT * FROM Students;

-- DELETE - remove Diana from the table
DELETE FROM Students
WHERE Id = 5;

-- See result after delete
SELECT * FROM Students;

-- ⚠️ WARNING: Never run these without a WHERE clause!
-- DELETE FROM Students       → deletes ALL rows
-- UPDATE Students SET Score = 0  → updates ALL rows
