// ============================================
// LESSON 14: Mini Project — Student Grade Tracker
// A complete console app using C# + SQL Server
// ============================================

// Phase 2 capstone. Combines everything:
//   Lesson  9: The database and Students table
//   Lesson 10: SqlConnection, SqlCommand, SqlDataReader
//   Lesson 11: INSERT, UPDATE, DELETE, ExecuteNonQuery
//   Lesson 12: WHERE, ORDER BY, aggregate functions
//   Lesson 13: Inline SQL with parameters (same pattern as stored procedures)

using Microsoft.Data.SqlClient;

string connectionString = "Server=localhost;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";
bool running = true;

Console.WriteLine("=========================================");
Console.WriteLine("       Student Grade Tracker");
Console.WriteLine("=========================================");
Console.WriteLine("");

while (running)
{
    ShowMenu();
    string choice = Console.ReadLine() ?? "";
    Console.WriteLine("");

    switch (choice)
    {
        case "1": ViewAllStudents(); break;
        case "2": AddStudent();      break;
        case "3": UpdateScore();     break;
        case "4": DeleteStudent();   break;
        case "5": SearchStudents();  break;
        case "6": ShowStatistics();  break;
        case "7":
            running = false;
            Console.WriteLine("Goodbye!");
            break;
        default:
            Console.WriteLine("Invalid choice. Enter 1 to 7.");
            break;
    }

    Console.WriteLine("");
}


// ============================================
// MENU
// ============================================

void ShowMenu()
{
    Console.WriteLine("-----------------------------------------");
    Console.WriteLine("  1 - View all students");
    Console.WriteLine("  2 - Add a student");
    Console.WriteLine("  3 - Update a score");
    Console.WriteLine("  4 - Delete a student");
    Console.WriteLine("  5 - Search by name");
    Console.WriteLine("  6 - Show statistics");
    Console.WriteLine("  7 - Quit");
    Console.WriteLine("-----------------------------------------");
    Console.Write("Your choice: ");
}


// ============================================
// 1. VIEW ALL — with letter grade calculated in SQL
// ============================================

void ViewAllStudents()
{
    using SqlConnection connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();

        // CASE expression in SQL works like if/else — computes Grade on the server
        string sql = @"
            SELECT Id, FirstName, LastName, Score,
                   CASE
                       WHEN Score >= 90 THEN 'A'
                       WHEN Score >= 80 THEN 'B'
                       WHEN Score >= 70 THEN 'C'
                       WHEN Score >= 60 THEN 'D'
                       ELSE                  'F'
                   END AS Grade
            FROM Students
            ORDER BY Score DESC";

        using SqlCommand command = new SqlCommand(sql, connection);
        using SqlDataReader reader = command.ExecuteReader();

        Console.WriteLine($"{"ID",-5} {"Name",-20} {"Score",-8} {"Grade"}");
        Console.WriteLine(new string('-', 40));

        int count = 0;
        while (reader.Read())
        {
            int id       = (int)reader["Id"];
            string name  = reader["FirstName"] + " " + reader["LastName"];
            int score    = (int)reader["Score"];
            string grade = (string)reader["Grade"];

            Console.WriteLine($"{id,-5} {name,-20} {score,-8} {grade}");
            count++;
        }

        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"  {count} student(s) total.");
    }
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}


// ============================================
// 2. ADD STUDENT
// ============================================

void AddStudent()
{
    Console.Write("First name   : ");
    string first = Console.ReadLine() ?? "";

    Console.Write("Last name    : ");
    string last = Console.ReadLine() ?? "";

    Console.Write("Email        : ");
    string email = Console.ReadLine() ?? "";

    Console.Write("Score (0-100): ");
    if (!int.TryParse(Console.ReadLine(), out int score) || score < 0 || score > 100)
    {
        Console.WriteLine("Invalid score. Must be a number between 0 and 100.");
        return;
    }

    using SqlConnection connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        string sql = @"
            INSERT INTO Students (FirstName, LastName, Email, Score)
            VALUES (@First, @Last, @Email, @Score);
            SELECT SCOPE_IDENTITY();";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@First", first);
        command.Parameters.AddWithValue("@Last",  last);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Score", score);

        int newId = Convert.ToInt32(command.ExecuteScalar());
        Console.WriteLine($"Student added! ID: {newId}");
    }
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}


// ============================================
// 3. UPDATE SCORE
// ============================================

void UpdateScore()
{
    Console.Write("Student ID to update: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Console.Write("New score (0-100): ");
    if (!int.TryParse(Console.ReadLine(), out int score) || score < 0 || score > 100)
    {
        Console.WriteLine("Invalid score.");
        return;
    }

    using SqlConnection connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        string sql = "UPDATE Students SET Score = @Score WHERE Id = @Id";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Score", score);
        command.Parameters.AddWithValue("@Id",    id);

        int rows = command.ExecuteNonQuery();
        Console.WriteLine(rows > 0 ? "Score updated!" : "No student found with that ID.");
    }
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}


// ============================================
// 4. DELETE STUDENT
// ============================================

void DeleteStudent()
{
    Console.Write("Student ID to delete: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Console.Write($"Are you sure you want to delete student {id}? (yes/no): ");
    if (Console.ReadLine() != "yes")
    {
        Console.WriteLine("Cancelled.");
        return;
    }

    using SqlConnection connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        string sql = "DELETE FROM Students WHERE Id = @Id";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        int rows = command.ExecuteNonQuery();
        Console.WriteLine(rows > 0 ? "Student deleted." : "No student found with that ID.");
    }
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}


// ============================================
// 5. SEARCH by first or last name
// ============================================

void SearchStudents()
{
    Console.Write("Search by name: ");
    string term = Console.ReadLine() ?? "";

    using SqlConnection connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        string sql = @"
            SELECT Id, FirstName, LastName, Score
            FROM Students
            WHERE FirstName LIKE @Term OR LastName LIKE @Term
            ORDER BY Score DESC";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Term", "%" + term + "%");

        using SqlDataReader reader = command.ExecuteReader();

        int count = 0;
        while (reader.Read())
        {
            Console.WriteLine($"  [{reader["Id"]}] {reader["FirstName"]} {reader["LastName"]} — Score: {reader["Score"]}");
            count++;
        }

        Console.WriteLine(count == 0 ? "  No results found." : $"  {count} result(s).");
    }
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}


// ============================================
// 6. STATISTICS — aggregates + grade breakdown
// ============================================

void ShowStatistics()
{
    using SqlConnection connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();

        // SUM(CASE WHEN ... THEN 1 ELSE 0 END) is the SQL pattern for counting by category
        string sql = @"
            SELECT
                COUNT(*)                                                      AS Total,
                AVG(CAST(Score AS FLOAT))                                     AS Average,
                MAX(Score)                                                    AS Highest,
                MIN(Score)                                                    AS Lowest,
                SUM(CASE WHEN Score >= 90                       THEN 1 ELSE 0 END) AS GradeA,
                SUM(CASE WHEN Score >= 80 AND Score < 90        THEN 1 ELSE 0 END) AS GradeB,
                SUM(CASE WHEN Score >= 70 AND Score < 80        THEN 1 ELSE 0 END) AS GradeC,
                SUM(CASE WHEN Score >= 60 AND Score < 70        THEN 1 ELSE 0 END) AS GradeD,
                SUM(CASE WHEN Score < 60                        THEN 1 ELSE 0 END) AS GradeF
            FROM Students";

        using SqlCommand command = new SqlCommand(sql, connection);
        using SqlDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            double avg = (double)reader["Average"];
            Console.WriteLine($"  Total students : {reader["Total"]}");
            Console.WriteLine($"  Average score  : {avg:F1}");
            Console.WriteLine($"  Highest score  : {reader["Highest"]}");
            Console.WriteLine($"  Lowest score   : {reader["Lowest"]}");
            Console.WriteLine($"");
            Console.WriteLine($"  Grade breakdown:");
            Console.WriteLine($"    A (90-100) : {reader["GradeA"]} student(s)");
            Console.WriteLine($"    B (80-89)  : {reader["GradeB"]} student(s)");
            Console.WriteLine($"    C (70-79)  : {reader["GradeC"]} student(s)");
            Console.WriteLine($"    D (60-69)  : {reader["GradeD"]} student(s)");
            Console.WriteLine($"    F (0-59)   : {reader["GradeF"]} student(s)");
        }
    }
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}
