// ============================================
// LESSON 19: Connect API to SQL
// Full CRUD API backed by a real database
// ============================================
//
// This replaces the in-memory list with real SQL Server queries.
// Every endpoint now reads from and writes to ChelloApp.Students.
//
// HOW TO TEST (REST Client in VS Code):
//   GET    http://localhost:5000/api/students
//   GET    http://localhost:5000/api/students/1
//   POST   http://localhost:5000/api/students     (with JSON body)
//   PUT    http://localhost:5000/api/students/1   (with JSON body)
//   DELETE http://localhost:5000/api/students/1

using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = "Server=localhost\\SQLEXPRESS;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";


// ============================================
// GET /api/students
// ============================================

app.MapGet("/api/students", () =>
{
    var students = new List<Student>();

    using var connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        var sql = "SELECT Id, FirstName, LastName, Email, Score FROM Students ORDER BY Score DESC";
        using var command = new SqlCommand(sql, connection);
        using var reader  = command.ExecuteReader();

        while (reader.Read())
            students.Add(ReadStudent(reader));

        return Results.Ok(students);
    }
    catch (SqlException ex)
    {
        return Results.Problem("Database error: " + ex.Message);
    }
});


// ============================================
// GET /api/students/{id}
// ============================================

app.MapGet("/api/students/{id}", (int id) =>
{
    using var connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        var sql = "SELECT Id, FirstName, LastName, Email, Score FROM Students WHERE Id = @Id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        using var reader = command.ExecuteReader();

        if (reader.Read())
            return Results.Ok(ReadStudent(reader));

        return Results.NotFound(new { Message = $"Student {id} not found." });
    }
    catch (SqlException ex)
    {
        return Results.Problem("Database error: " + ex.Message);
    }
});


// ============================================
// POST /api/students
// ============================================

app.MapPost("/api/students", (StudentRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
        return Results.BadRequest(new { Message = "FirstName and LastName are required." });

    if (req.Score < 0 || req.Score > 100)
        return Results.BadRequest(new { Message = "Score must be 0-100." });

    using var connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        var sql = @"INSERT INTO Students (FirstName, LastName, Email, Score)
                    VALUES (@First, @Last, @Email, @Score);
                    SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@First", req.FirstName);
        command.Parameters.AddWithValue("@Last",  req.LastName);
        command.Parameters.AddWithValue("@Email", req.Email);
        command.Parameters.AddWithValue("@Score", req.Score);

        int newId = Convert.ToInt32(command.ExecuteScalar());

        var created = new Student
        {
            Id = newId, FirstName = req.FirstName, LastName = req.LastName,
            Email = req.Email, Score = req.Score
        };

        return Results.Created($"/api/students/{newId}", created);
    }
    catch (SqlException ex)
    {
        return Results.Problem("Database error: " + ex.Message);
    }
});


// ============================================
// PUT /api/students/{id}
// ============================================

app.MapPut("/api/students/{id}", (int id, StudentRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
        return Results.BadRequest(new { Message = "FirstName and LastName are required." });

    if (req.Score < 0 || req.Score > 100)
        return Results.BadRequest(new { Message = "Score must be 0-100." });

    using var connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        var sql = @"UPDATE Students
                    SET FirstName = @First, LastName = @Last, Email = @Email, Score = @Score
                    WHERE Id = @Id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@First", req.FirstName);
        command.Parameters.AddWithValue("@Last",  req.LastName);
        command.Parameters.AddWithValue("@Email", req.Email);
        command.Parameters.AddWithValue("@Score", req.Score);
        command.Parameters.AddWithValue("@Id",    id);

        int rows = command.ExecuteNonQuery();

        if (rows == 0)
            return Results.NotFound(new { Message = $"Student {id} not found." });

        var updated = new Student
        {
            Id = id, FirstName = req.FirstName, LastName = req.LastName,
            Email = req.Email, Score = req.Score
        };

        return Results.Ok(updated);
    }
    catch (SqlException ex)
    {
        return Results.Problem("Database error: " + ex.Message);
    }
});


// ============================================
// DELETE /api/students/{id}
// ============================================

app.MapDelete("/api/students/{id}", (int id) =>
{
    using var connection = new SqlConnection(connectionString);
    try
    {
        connection.Open();
        var sql = "DELETE FROM Students WHERE Id = @Id";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        int rows = command.ExecuteNonQuery();

        if (rows == 0)
            return Results.NotFound(new { Message = $"Student {id} not found." });

        return Results.NoContent();
    }
    catch (SqlException ex)
    {
        return Results.Problem("Database error: " + ex.Message);
    }
});


app.Run();


// ============================================
// HELPERS
// ============================================

Student ReadStudent(SqlDataReader reader) => new Student
{
    Id        = (int)reader["Id"],
    FirstName = (string)reader["FirstName"],
    LastName  = (string)reader["LastName"],
    Email     = (string)reader["Email"],
    Score     = (int)reader["Score"]
};


class Student
{
    public int    Id        { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName  { get; set; } = "";
    public string Email     { get; set; } = "";
    public int    Score     { get; set; }
    public string Grade => Score switch
    {
        >= 90 => "A", >= 80 => "B", >= 70 => "C", >= 60 => "D", _ => "F"
    };
}

class StudentRequest
{
    public string FirstName { get; set; } = "";
    public string LastName  { get; set; } = "";
    public string Email     { get; set; } = "";
    public int    Score     { get; set; }
}
