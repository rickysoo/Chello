// ============================================
// LESSON 11: CRUD Operations
// Create, Read, Update, Delete with C# + SQL
// ============================================

// In Lesson 10 we learned to READ (SELECT)
// Now we cover the other three:
//   CREATE → INSERT
//   UPDATE → UPDATE
//   DELETE → DELETE

using Microsoft.Data.SqlClient;

string connectionString = "Server=localhost;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";

Console.WriteLine("=== Lesson 11: CRUD Operations ===");
Console.WriteLine("");

// ============================================
// CREATE — INSERT a new student
// ============================================

Console.WriteLine("--- CREATE: Add a new student ---");
int newId = InsertStudent(connectionString, "Eve", "Tan", "eve@email.com", 82);
if (newId > 0)
    Console.WriteLine("New student added with ID: " + newId);

Console.WriteLine("");

// ============================================
// READ — SELECT all students (recap from Lesson 10)
// ============================================

Console.WriteLine("--- READ: All students ---");
ReadAllStudents(connectionString);

Console.WriteLine("");

// ============================================
// UPDATE — change Eve's score
// ============================================

Console.WriteLine("--- UPDATE: Change Eve's score to 90 ---");
int rowsUpdated = UpdateScore(connectionString, newId, 90);
Console.WriteLine("Rows updated: " + rowsUpdated);

Console.WriteLine("");

Console.WriteLine("--- READ: Eve's record after update ---");
ReadStudentById(connectionString, newId);

Console.WriteLine("");

// ============================================
// DELETE — remove the student we just added
// ============================================

Console.Write("Delete the student we just added? (yes/no): ");
string confirm = Console.ReadLine() ?? "";

if (confirm == "yes")
{
    int rowsDeleted = DeleteStudent(connectionString, newId);
    Console.WriteLine("Rows deleted: " + rowsDeleted);
    Console.WriteLine("Student removed.");
}
else
{
    Console.WriteLine("Skipped delete.");
}

Console.WriteLine("");
Console.WriteLine("CRUD complete! You can now Create, Read, Update, and Delete database records.");


// ============================================
// INSERT — returns the new row's ID
// ============================================

int InsertStudent(string connStr, string firstName, string lastName, string email, int score)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();

        // Run INSERT then immediately SELECT SCOPE_IDENTITY() to get the new auto-generated ID
        string sql = @"
            INSERT INTO Students (FirstName, LastName, Email, Score)
            VALUES (@First, @Last, @Email, @Score);
            SELECT SCOPE_IDENTITY();";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@First", firstName);
        command.Parameters.AddWithValue("@Last",  lastName);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Score", score);

        // ExecuteScalar() runs the query and returns the first column of the first row —
        // exactly what we need to capture the new ID
        object result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }
    catch (SqlException ex)
    {
        Console.WriteLine("Insert error: " + ex.Message);
        return -1;
    }
}


// ============================================
// SELECT all students
// ============================================

void ReadAllStudents(string connStr)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        string sql = "SELECT Id, FirstName, LastName, Email, Score FROM Students ORDER BY Id";

        using SqlCommand command = new SqlCommand(sql, connection);
        using SqlDataReader reader = command.ExecuteReader();

        Console.WriteLine($"{"ID",-5} {"Name",-20} {"Email",-25} {"Score"}");
        Console.WriteLine(new string('-', 57));

        while (reader.Read())
        {
            int id       = (int)reader["Id"];
            string name  = reader["FirstName"] + " " + reader["LastName"];
            string email = (string)reader["Email"];
            int score    = (int)reader["Score"];

            Console.WriteLine($"{id,-5} {name,-20} {email,-25} {score}");
        }

        Console.WriteLine(new string('-', 57));
    }
    catch (SqlException ex) { Console.WriteLine("Read error: " + ex.Message); }
}


// ============================================
// SELECT one student by ID
// ============================================

void ReadStudentById(string connStr, int id)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        string sql = "SELECT Id, FirstName, LastName, Email, Score FROM Students WHERE Id = @Id";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        using SqlDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            Console.WriteLine($"  ID:    {reader["Id"]}");
            Console.WriteLine($"  Name:  {reader["FirstName"]} {reader["LastName"]}");
            Console.WriteLine($"  Email: {reader["Email"]}");
            Console.WriteLine($"  Score: {reader["Score"]}");
        }
        else
        {
            Console.WriteLine("  Student not found.");
        }
    }
    catch (SqlException ex) { Console.WriteLine("Read error: " + ex.Message); }
}


// ============================================
// UPDATE — returns number of rows affected
// ============================================

int UpdateScore(string connStr, int studentId, int newScore)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        string sql = "UPDATE Students SET Score = @Score WHERE Id = @Id";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Score", newScore);
        command.Parameters.AddWithValue("@Id",    studentId);

        // ExecuteNonQuery() runs INSERT / UPDATE / DELETE
        // It does not return rows — it returns the count of rows affected
        return command.ExecuteNonQuery();
    }
    catch (SqlException ex)
    {
        Console.WriteLine("Update error: " + ex.Message);
        return 0;
    }
}


// ============================================
// DELETE — returns number of rows affected
// ============================================

int DeleteStudent(string connStr, int studentId)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        string sql = "DELETE FROM Students WHERE Id = @Id";

        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", studentId);

        return command.ExecuteNonQuery();
    }
    catch (SqlException ex)
    {
        Console.WriteLine("Delete error: " + ex.Message);
        return 0;
    }
}
