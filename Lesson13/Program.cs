// ============================================
// LESSON 13: Stored Procedures
// Call pre-written SQL logic from C#
// ============================================

// A stored procedure is SQL code saved inside the database itself.
// Instead of writing SQL strings in C#, you call the procedure by name.
//
// Benefits:
//   - SQL logic lives in one place (the DB), not scattered across code files
//   - SQL Server compiles and caches the plan — faster repeated calls
//   - Change DB behaviour without redeploying the C# app
//
// BEFORE RUNNING:
//   Open 01_CreateStoredProcedures.sql in SSMS and execute it first.

using Microsoft.Data.SqlClient;
using System.Data;

string connectionString = "Server=localhost;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";

Console.WriteLine("=== Lesson 13: Stored Procedures ===");
Console.WriteLine("");

// ---- Call sp_GetAllStudents (no parameters) ----
Console.WriteLine("--- All students (via stored procedure) ---");
GetAllStudents(connectionString);
Console.WriteLine("");

// ---- Call sp_AddStudent with an OUTPUT parameter ----
Console.WriteLine("--- Add a student (via stored procedure) ---");
int newId = AddStudent(connectionString, "Frank", "Ng", "frank@email.com", 78);
Console.WriteLine("New student ID returned from SQL Server: " + newId);
Console.WriteLine("");

// ---- Call sp_GetStudentById ----
Console.WriteLine("--- Get student by ID ---");
GetStudentById(connectionString, newId);
Console.WriteLine("");

// ---- Call sp_UpdateScore ----
Console.WriteLine("--- Update Frank's score to 85 ---");
int rowsAffected = UpdateScore(connectionString, newId, 85);
Console.WriteLine("Rows affected: " + rowsAffected);
Console.WriteLine("");

Console.WriteLine("--- Frank after update ---");
GetStudentById(connectionString, newId);

Console.WriteLine("");
Console.WriteLine("Done! Stored procedures let your database do the heavy lifting.");


// ============================================
// sp_GetAllStudents — no parameters
// ============================================

void GetAllStudents(string connStr)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        using SqlCommand command = new SqlCommand("sp_GetAllStudents", connection);

        // This one line tells SqlCommand it's calling a stored procedure, not inline SQL
        command.CommandType = CommandType.StoredProcedure;

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
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}


// ============================================
// sp_GetStudentById — one input parameter
// ============================================

void GetStudentById(string connStr, int studentId)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        using SqlCommand command = new SqlCommand("sp_GetStudentById", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@StudentId", studentId);

        using SqlDataReader reader = command.ExecuteReader();

        if (reader.Read())
            Console.WriteLine($"  [{reader["Id"]}] {reader["FirstName"]} {reader["LastName"]} — Score: {reader["Score"]}");
        else
            Console.WriteLine("  Not found.");
    }
    catch (SqlException ex) { Console.WriteLine("Error: " + ex.Message); }
}


// ============================================
// sp_AddStudent — input params + OUTPUT param
// ============================================

int AddStudent(string connStr, string firstName, string lastName, string email, int score)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        using SqlCommand command = new SqlCommand("sp_AddStudent", connection);
        command.CommandType = CommandType.StoredProcedure;

        // Regular input parameters
        command.Parameters.AddWithValue("@FirstName", firstName);
        command.Parameters.AddWithValue("@LastName",  lastName);
        command.Parameters.AddWithValue("@Email",     email);
        command.Parameters.AddWithValue("@Score",     score);

        // OUTPUT parameter — SQL Server writes the new ID into this after the INSERT
        SqlParameter outputParam = new SqlParameter("@NewId", SqlDbType.Int);
        outputParam.Direction = ParameterDirection.Output;
        command.Parameters.Add(outputParam);

        command.ExecuteNonQuery();

        // After execution, the value SQL Server wrote is now available here
        return (int)outputParam.Value;
    }
    catch (SqlException ex)
    {
        Console.WriteLine("Error: " + ex.Message);
        return -1;
    }
}


// ============================================
// sp_UpdateScore — procedure returns rows affected via SELECT
// ============================================

int UpdateScore(string connStr, int studentId, int newScore)
{
    using SqlConnection connection = new SqlConnection(connStr);
    try
    {
        connection.Open();
        using SqlCommand command = new SqlCommand("sp_UpdateScore", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@StudentId", studentId);
        command.Parameters.AddWithValue("@NewScore",  newScore);

        // The procedure does SELECT @@ROWCOUNT, so ExecuteScalar reads that single value back
        object result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }
    catch (SqlException ex)
    {
        Console.WriteLine("Error: " + ex.Message);
        return 0;
    }
}
