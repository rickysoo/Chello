// ============================================================
// Lesson 33: Full Stack Project Setup
// Backend: ASP.NET Web API with CORS for React frontend
// ============================================================
//
// This API serves student data to our React frontend.
// It connects to SQL Server and exposes 5 CRUD endpoints.
//
// KEY CONCEPT: CORS (Cross-Origin Resource Sharing)
// -------------------------------------------------
// Our React app runs on localhost:3000
// Our API runs on localhost:5000
// These are DIFFERENT origins (different ports = different origin)
//
// Browsers block cross-origin requests by default for security.
// We must explicitly tell the API to ALLOW requests from localhost:3000.
// That is what the CORS configuration below does.
//
// Without it, the browser throws:
//   "Access to fetch has been blocked by CORS policy"

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// STEP 1: Configure CORS
// Allow our React frontend (localhost:3000) to call this API
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",   // React dev server
                "null"                     // Allows file:// opened HTML files
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ============================================================
// STEP 2: Enable CORS middleware
// This MUST come before your route definitions
// ============================================================
app.UseCors("AllowReactApp");

// ============================================================
// Database connection string
// Points to local SQL Server Express, ChelloApp database
// ============================================================
string connectionString = @"Server=localhost\SQLEXPRESS;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";

// ============================================================
// Helper: Open a database connection
// ============================================================
SqlConnection GetConnection() => new SqlConnection(connectionString);

// ============================================================
// ENDPOINT 1: GET /api/students
// Returns all students as a JSON array
// ============================================================
app.MapGet("/api/students", async () =>
{
    var students = new List<object>();

    using var conn = GetConnection();
    await conn.OpenAsync();

    var cmd = new SqlCommand("SELECT Id, Name, Email, Age FROM Students ORDER BY Id", conn);
    using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        students.Add(new
        {
            id    = reader.GetInt32(0),
            name  = reader.GetString(1),
            email = reader.GetString(2),
            age   = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
        });
    }

    return Results.Ok(students);
});

// ============================================================
// ENDPOINT 2: GET /api/students/{id}
// Returns a single student by their ID
// ============================================================
app.MapGet("/api/students/{id}", async (int id) =>
{
    using var conn = GetConnection();
    await conn.OpenAsync();

    var cmd = new SqlCommand("SELECT Id, Name, Email, Age FROM Students WHERE Id = @Id", conn);
    cmd.Parameters.AddWithValue("@Id", id);

    using var reader = await cmd.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        var student = new
        {
            id    = reader.GetInt32(0),
            name  = reader.GetString(1),
            email = reader.GetString(2),
            age   = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
        };
        return Results.Ok(student);
    }

    return Results.NotFound(new { message = $"Student with ID {id} not found." });
});

// ============================================================
// ENDPOINT 3: POST /api/students
// Creates a new student from JSON body
// Expected JSON: { "name": "...", "email": "...", "age": 25 }
// ============================================================
app.MapPost("/api/students", async (HttpRequest request) =>
{
    // Read and parse the JSON body
    using var body = await JsonDocument.ParseAsync(request.Body);
    var root = body.RootElement;

    string name  = root.GetProperty("name").GetString() ?? "";
    string email = root.GetProperty("email").GetString() ?? "";
    int?   age   = root.TryGetProperty("age", out var ageProp) ? ageProp.GetInt32() : null;

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest(new { message = "Name and Email are required." });
    }

    using var conn = GetConnection();
    await conn.OpenAsync();

    var cmd = new SqlCommand(
        "INSERT INTO Students (Name, Email, Age) OUTPUT INSERTED.Id VALUES (@Name, @Email, @Age)",
        conn
    );
    cmd.Parameters.AddWithValue("@Name",  name);
    cmd.Parameters.AddWithValue("@Email", email);
    cmd.Parameters.AddWithValue("@Age",   (object?)age ?? DBNull.Value);

    int newId = (int)(await cmd.ExecuteScalarAsync())!;

    return Results.Created($"/api/students/{newId}", new { id = newId, name, email, age });
});

// ============================================================
// ENDPOINT 4: PUT /api/students/{id}
// Updates an existing student
// Expected JSON: { "name": "...", "email": "...", "age": 30 }
// ============================================================
app.MapPut("/api/students/{id}", async (int id, HttpRequest request) =>
{
    using var body = await JsonDocument.ParseAsync(request.Body);
    var root = body.RootElement;

    string name  = root.GetProperty("name").GetString() ?? "";
    string email = root.GetProperty("email").GetString() ?? "";
    int?   age   = root.TryGetProperty("age", out var ageProp) ? ageProp.GetInt32() : null;

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest(new { message = "Name and Email are required." });
    }

    using var conn = GetConnection();
    await conn.OpenAsync();

    var cmd = new SqlCommand(
        "UPDATE Students SET Name = @Name, Email = @Email, Age = @Age WHERE Id = @Id",
        conn
    );
    cmd.Parameters.AddWithValue("@Name",  name);
    cmd.Parameters.AddWithValue("@Email", email);
    cmd.Parameters.AddWithValue("@Age",   (object?)age ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@Id",    id);

    int rowsAffected = await cmd.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
    {
        return Results.NotFound(new { message = $"Student with ID {id} not found." });
    }

    return Results.Ok(new { id, name, email, age });
});

// ============================================================
// ENDPOINT 5: DELETE /api/students/{id}
// Deletes a student by their ID
// ============================================================
app.MapDelete("/api/students/{id}", async (int id) =>
{
    using var conn = GetConnection();
    await conn.OpenAsync();

    var cmd = new SqlCommand("DELETE FROM Students WHERE Id = @Id", conn);
    cmd.Parameters.AddWithValue("@Id", id);

    int rowsAffected = await cmd.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
    {
        return Results.NotFound(new { message = $"Student with ID {id} not found." });
    }

    return Results.Ok(new { message = $"Student {id} deleted successfully." });
});

// ============================================================
// Start the app on port 5000
// ============================================================
app.Run("http://localhost:5000");
