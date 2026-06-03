// ============================================
// LESSON 10: C# Connects to SQL
// Read data from the database using C#
// ============================================

// WHAT YOU NEED BEFORE RUNNING THIS:
//   1. SQL Server installed and running
//   2. Run all 5 scripts from Lesson 9 (creates ChelloApp DB + Students table + data)
//   3. Update the connection string below to match your server name

using Microsoft.Data.SqlClient;

// ============================================
// STEP 1: The Connection String
// This tells C# WHERE the database is and HOW to connect
// ============================================

// Replace "localhost" with your SQL Server name if different
// To find your server name: open SQL Server Management Studio and look at the top of the login window
string connectionString = "Server=localhost;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";

// What each part means:
//   Server=localhost         → the machine where SQL Server is running
//   Database=ChelloApp       → the database we created in Lesson 9
//   Trusted_Connection=True  → use Windows login (no username/password needed)
//   TrustServerCertificate   → skip SSL certificate check (fine for local dev)

Console.WriteLine("=== Lesson 10: C# Connects to SQL ===");
Console.WriteLine("");

// ============================================
// STEP 2: Connect and Read All Students
// ============================================

Console.WriteLine("--- All Students ---");
ReadAllStudents(connectionString);

Console.WriteLine("");

// ============================================
// STEP 3: Search by name
// ============================================

Console.Write("Search for a student by first name: ");
string searchName = Console.ReadLine() ?? "";

Console.WriteLine("");
Console.WriteLine("--- Search Results ---");
SearchStudents(connectionString, searchName);

Console.WriteLine("");
Console.WriteLine("Done! You just read from a real database using C#.");


// ============================================
// METHOD: Read all students from the DB
// ============================================

void ReadAllStudents(string connStr)
{
    // SqlConnection opens the connection to SQL Server
    // The 'using' block automatically closes it when done — always do this!
    using (SqlConnection connection = new SqlConnection(connStr))
    {
        try
        {
            connection.Open();  // actually connects to the database
            Console.WriteLine("Connected to database!");
            Console.WriteLine("");

            // SqlCommand holds the SQL query you want to run
            string sql = "SELECT Id, FirstName, LastName, Email, Score FROM Students ORDER BY Score DESC";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                // SqlDataReader reads the results row by row
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    // Print a header
                    Console.WriteLine($"{"ID",-5} {"Name",-20} {"Email",-25} {"Score",-5}");
                    Console.WriteLine(new string('-', 57));

                    // reader.Read() moves to the next row — returns false when no more rows
                    while (reader.Read())
                    {
                        // reader["ColumnName"] gets the value from that column
                        int id         = (int)reader["Id"];
                        string first   = (string)reader["FirstName"];
                        string last    = (string)reader["LastName"];
                        string email   = (string)reader["Email"];
                        int score      = (int)reader["Score"];

                        string fullName = first + " " + last;
                        Console.WriteLine($"{id,-5} {fullName,-20} {email,-25} {score,-5}");
                    }

                    Console.WriteLine(new string('-', 57));
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Database error: " + ex.Message);
            Console.WriteLine("");
            Console.WriteLine("Tip: Make sure SQL Server is running and your connection string is correct.");
        }
    }
}


// ============================================
// METHOD: Search for students by first name
// Uses a parameter (@Name) to safely insert user input into SQL
// ============================================

void SearchStudents(string connStr, string name)
{
    using (SqlConnection connection = new SqlConnection(connStr))
    {
        try
        {
            connection.Open();

            // IMPORTANT: Never write user input directly into SQL like this:
            //   "SELECT * FROM Students WHERE FirstName = '" + name + "'"  ← BAD! SQL injection risk!
            //
            // Instead, use a parameter (@Name). SQL Server fills it in safely.
            string sql = "SELECT Id, FirstName, LastName, Score FROM Students WHERE FirstName LIKE @Name";

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                // Add the parameter — the % symbols mean "anything before/after"
                command.Parameters.AddWithValue("@Name", "%" + name + "%");

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    int count = 0;

                    while (reader.Read())
                    {
                        int id       = (int)reader["Id"];
                        string first = (string)reader["FirstName"];
                        string last  = (string)reader["LastName"];
                        int score    = (int)reader["Score"];

                        Console.WriteLine($"  [{id}] {first} {last} — Score: {score}");
                        count++;
                    }

                    if (count == 0)
                    {
                        Console.WriteLine("  No students found matching: " + name);
                    }
                    else
                    {
                        Console.WriteLine("");
                        Console.WriteLine("  Found " + count + " student(s).");
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine("Database error: " + ex.Message);
        }
    }
}
