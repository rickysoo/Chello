// ============================================
// LESSON 39: Deploy Backend — Publish C# API to Railway
// Production-ready Students API
// ============================================
//
// Key changes from Lesson 19:
//   1. Connection string read from environment variable (DATABASE_URL)
//   2. CORS configured to allow the Vercel frontend domain
//   3. Health check endpoint GET /health
//   4. App listens on the PORT env var that Railway provides
//   5. Graceful error if required env vars are missing
//
// Environment variables required on Railway:
//   DATABASE_URL  — SQL Server or PostgreSQL connection string
//   ALLOWED_ORIGIN — your Vercel frontend URL, e.g. https://my-app.vercel.app
//   (JWT_SECRET   — add this when you integrate auth in a later lesson)

using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Read environment variables
// ============================================

string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException(
        "DATABASE_URL environment variable is not set. " +
        "Add it in Railway → your service → Variables.");

string allowedOrigin = Environment.GetEnvironmentVariable("ALLOWED_ORIGIN")
    ?? "*";   // falls back to allow all origins if not set

// ============================================
// CORS — allow the Vercel frontend to call this API
// ============================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if (allowedOrigin == "*")
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            policy.WithOrigins(allowedOrigin)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

// ============================================
// GET /health
// Railway (and other platforms) ping this to check the app is alive
// ============================================

app.MapGet("/health", () => Results.Ok(new { Status = "OK", Time = DateTime.UtcNow }));


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
        command.Parameters.AddWithValue("@Email", req.Email ?? "");
        command.Parameters.AddWithValue("@Score", req.Score);

        int newId = Convert.ToInt32(command.ExecuteScalar());

        var created = new Student
        {
            Id = newId, FirstName = req.FirstName, LastName = req.LastName,
            Email = req.Email ?? "", Score = req.Score
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
        command.Parameters.AddWithValue("@Email", req.Email ?? "");
        command.Parameters.AddWithValue("@Score", req.Score);
        command.Parameters.AddWithValue("@Id",    id);

        int rows = command.ExecuteNonQuery();

        if (rows == 0)
            return Results.NotFound(new { Message = $"Student {id} not found." });

        var updated = new Student
        {
            Id = id, FirstName = req.FirstName, LastName = req.LastName,
            Email = req.Email ?? "", Score = req.Score
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


// ============================================
// Start — Railway injects PORT; fall back to 5000 locally
// ============================================

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");


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
    public string  FirstName { get; set; } = "";
    public string  LastName  { get; set; } = "";
    public string? Email     { get; set; }
    public int     Score     { get; set; }
}
