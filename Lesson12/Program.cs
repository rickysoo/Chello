// ============================================
// LESSON 12: SQL Queries
// Filter, sort, and analyse data from C#
// ============================================

// SQL can do far more than "get all rows".
// This lesson covers:
//   WHERE  — filter rows by conditions
//   AND / OR / BETWEEN — combine conditions
//   ORDER BY — sort results
//   TOP    — limit how many rows come back
//   COUNT, AVG, MAX, MIN — aggregate functions

using Microsoft.Data.SqlClient;

string connectionString = "Server=localhost;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";

Console.WriteLine("=== Lesson 12: SQL Queries ===");
Console.WriteLine("");

// ---- Students who passed (score >= 60) ----
Console.WriteLine("--- Students who passed (score >= 60) ---");
RunQuery(connectionString,
    "SELECT FirstName, LastName, Score FROM Students WHERE Score >= 60 ORDER BY Score DESC");

Console.WriteLine("");

// ---- Score in a range ----
Console.WriteLine("--- Students scoring between 70 and 90 ---");
RunQueryWithParams(connectionString,
    "SELECT FirstName, LastName, Score FROM Students WHERE Score BETWEEN @Min AND @Max ORDER BY Score DESC",
    ("@Min", 70), ("@Max", 90));

Console.WriteLine("");

// ---- Top 3 performers ----
Console.WriteLine("--- Top 3 students ---");
RunQuery(connectionString,
    "SELECT TOP 3 FirstName, LastName, Score FROM Students ORDER BY Score DESC");

Console.WriteLine("");

// ---- Class statistics ----
Console.WriteLine("--- Class Statistics ---");
ShowStatistics(connectionString);

Console.WriteLine("");

// ---- Live filter from user input ----
Console.Write("Show students scoring above: ");
string input = Console.ReadLine() ?? "0";

if (int.TryParse(input, out int minScore))
{
    Console.WriteLine("");
    Console.WriteLine($"--- Students scoring above {minScore} ---");
    RunQueryWithParams(connectionString,
        "SELECT FirstName, LastName, Score FROM Students WHERE Score > @Min ORDER BY Score DESC",
        ("@Min", minScore));
}
else
{
    Console.WriteLine("Not a valid number, skipping.");
}

Console.WriteLine("");
Console.WriteLine("Done! You can now filter, sort, and summarise database data.");


// ============================================
// Run a plain SELECT and print all columns
// ============================================

void RunQuery(string connStr, string sql)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        using SqlCommand command = new SqlCommand(sql, connection);
        using SqlDataReader reader = command.ExecuteReader();
        PrintResults(reader);
    }
    catch (SqlException ex) { Console.WriteLine("Query error: " + ex.Message); }
}


// ============================================
// Run a SELECT with named parameters
// Uses C# tuple params so callers stay readable
// ============================================

void RunQueryWithParams(string connStr, string sql, params (string name, object value)[] parameters)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        using SqlCommand command = new SqlCommand(sql, connection);

        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);

        using SqlDataReader reader = command.ExecuteReader();
        PrintResults(reader);
    }
    catch (SqlException ex) { Console.WriteLine("Query error: " + ex.Message); }
}


// ============================================
// Print every row from a reader generically
// ============================================

void PrintResults(SqlDataReader reader)
{
    int count = 0;

    while (reader.Read())
    {
        var parts = new List<string>();
        for (int i = 0; i < reader.FieldCount; i++)
            parts.Add(reader.GetName(i) + ": " + reader.GetValue(i));

        Console.WriteLine("  " + string.Join(" | ", parts));
        count++;
    }

    if (count == 0)
        Console.WriteLine("  No results.");
    else
        Console.WriteLine("  (" + count + " row(s))");
}


// ============================================
// Aggregate functions: COUNT, AVG, MAX, MIN
// ============================================

void ShowStatistics(string connStr)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();

        // CAST(Score AS FLOAT) makes AVG return a decimal, not a truncated integer
        string sql = @"
            SELECT
                COUNT(*)              AS TotalStudents,
                AVG(CAST(Score AS FLOAT)) AS AverageScore,
                MAX(Score)            AS HighestScore,
                MIN(Score)            AS LowestScore
            FROM Students";

        using SqlCommand command = new SqlCommand(sql, connection);
        using SqlDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            double avg = (double)reader["AverageScore"];
            Console.WriteLine("  Total students : " + reader["TotalStudents"]);
            Console.WriteLine("  Average score  : " + avg.ToString("F1"));
            Console.WriteLine("  Highest score  : " + reader["HighestScore"]);
            Console.WriteLine("  Lowest score   : " + reader["LowestScore"]);
        }
    }
    catch (SqlException ex) { Console.WriteLine("Statistics error: " + ex.Message); }
}
