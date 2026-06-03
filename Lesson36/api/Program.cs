// ============================================================
// Lesson 36: Dashboard — Show User Data from DB
// ============================================================
// Topics covered:
//   - ASP.NET Web API with JWT authentication
//   - CORS (Cross-Origin Resource Sharing) setup
//   - Protected endpoints with RequireAuthorization()
//   - Full CRUD for Students (GET, POST, PUT, DELETE)
//   - Dashboard stats endpoint: totals, averages, grade breakdown
//   - SQLite database with Microsoft.Data.Sqlite
//   - Generating and validating JWT tokens
// ============================================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// STEP 1: Configure JWT Authentication
// ============================================================
// JWT (JSON Web Token) is a compact, URL-safe way to represent
// claims between two parties. When a user logs in, the server
// creates a signed token. The client sends this token with
// every request to prove who they are.

const string JwtSecretKey = "lesson36-super-secret-key-for-demo-only-32chars!";
const string JwtIssuer = "lesson36-api";
const string JwtAudience = "lesson36-frontend";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtIssuer,
            ValidAudience = JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey))
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// STEP 2: Configure CORS
// ============================================================
// CORS allows our frontend (running on a different port or domain)
// to call our API. Without this, the browser blocks the request.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()   // In production, use specific origins
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ============================================================
// STEP 3: Set up the SQLite database
// ============================================================
// We create the database and seed it with sample data when the
// API starts. This keeps the lesson self-contained.

const string DbPath = "lesson36.db";
string ConnectionString = $"Data Source={DbPath}";

void InitializeDatabase()
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    // Create students table
    var createTable = connection.CreateCommand();
    createTable.CommandText = @"
        CREATE TABLE IF NOT EXISTS Students (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Email TEXT NOT NULL,
            Score INTEGER NOT NULL,
            Course TEXT NOT NULL,
            EnrolledDate TEXT NOT NULL
        );
    ";
    createTable.ExecuteNonQuery();

    // Check if we already have data (avoid duplicate seeding)
    var countCmd = connection.CreateCommand();
    countCmd.CommandText = "SELECT COUNT(*) FROM Students;";
    var count = (long)(countCmd.ExecuteScalar() ?? 0);

    if (count == 0)
    {
        // Seed with sample student data
        var seedData = new[]
        {
            ("Alice Johnson",    "alice@example.com",   92, "C# Fundamentals",  "2024-01-15"),
            ("Bob Smith",        "bob@example.com",     78, "Web Development",  "2024-01-20"),
            ("Carol Davis",      "carol@example.com",   95, "C# Fundamentals",  "2024-02-01"),
            ("David Lee",        "david@example.com",   65, "Data Science",     "2024-02-10"),
            ("Emma Wilson",      "emma@example.com",    88, "Web Development",  "2024-02-15"),
            ("Frank Brown",      "frank@example.com",   72, "C# Fundamentals",  "2024-03-01"),
            ("Grace Kim",        "grace@example.com",   91, "Data Science",     "2024-03-05"),
            ("Henry Taylor",     "henry@example.com",   55, "Web Development",  "2024-03-10"),
            ("Iris Martinez",    "iris@example.com",    83, "C# Fundamentals",  "2024-03-20"),
            ("Jack Anderson",    "jack@example.com",    97, "Data Science",     "2024-04-01"),
        };

        foreach (var (name, email, score, course, date) in seedData)
        {
            var insert = connection.CreateCommand();
            insert.CommandText = @"
                INSERT INTO Students (Name, Email, Score, Course, EnrolledDate)
                VALUES ($name, $email, $score, $course, $date);
            ";
            insert.Parameters.AddWithValue("$name", name);
            insert.Parameters.AddWithValue("$email", email);
            insert.Parameters.AddWithValue("$score", score);
            insert.Parameters.AddWithValue("$course", course);
            insert.Parameters.AddWithValue("$date", date);
            insert.ExecuteNonQuery();
        }

        Console.WriteLine("Database seeded with 10 sample students.");
    }
}

InitializeDatabase();

// ============================================================
// STEP 4: Apply middleware in the correct order
// ============================================================
// Order matters! CORS must come before auth, auth before routing.

app.UseCors("AllowFrontend");
app.UseAuthentication();    // "Who are you?" — reads the JWT token
app.UseAuthorization();     // "Are you allowed?" — checks permissions

// ============================================================
// STEP 5: Auth endpoint — Login (public, no token needed)
// ============================================================

// Helper: generate a JWT token for a given username
string GenerateJwtToken(string username)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, "admin"),
    };

    var token = new JwtSecurityToken(
        issuer: JwtIssuer,
        audience: JwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),   // Token lasts 8 hours
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// POST /api/auth/login
// Body: { "username": "admin", "password": "password123" }
// Returns a JWT token on success
app.MapPost("/api/auth/login", (LoginRequest request) =>
{
    Console.WriteLine($"Login attempt: username='{request.Username}'");

    // Hardcoded credentials for demo purposes
    // In a real app, you would check against a database
    if (request.Username == "admin" && request.Password == "password123")
    {
        var token = GenerateJwtToken(request.Username);
        return Results.Ok(new { token, username = request.Username, message = "Login successful" });
    }

    return Results.Unauthorized();
});

// ============================================================
// STEP 6: Student CRUD endpoints — all PROTECTED
// ============================================================
// RequireAuthorization() means the client must include a valid
// JWT token in the Authorization header:
//   Authorization: Bearer <token>

// GET /api/students — get all students
app.MapGet("/api/students", () =>
{
    var students = new List<Student>();

    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT Id, Name, Email, Score, Course, EnrolledDate FROM Students ORDER BY Id;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        students.Add(new Student(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetString(4),
            reader.GetString(5)
        ));
    }

    return Results.Ok(students);
})
.RequireAuthorization();

// GET /api/students/{id} — get one student by ID
app.MapGet("/api/students/{id}", (int id) =>
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT Id, Name, Email, Score, Course, EnrolledDate FROM Students WHERE Id = $id;";
    cmd.Parameters.AddWithValue("$id", id);

    using var reader = cmd.ExecuteReader();
    if (reader.Read())
    {
        var student = new Student(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetString(4),
            reader.GetString(5)
        );
        return Results.Ok(student);
    }

    return Results.NotFound(new { message = $"Student with ID {id} not found." });
})
.RequireAuthorization();

// POST /api/students — create a new student
app.MapPost("/api/students", (CreateStudentRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        return Results.BadRequest(new { message = "Name and Email are required." });

    if (request.Score < 0 || request.Score > 100)
        return Results.BadRequest(new { message = "Score must be between 0 and 100." });

    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Students (Name, Email, Score, Course, EnrolledDate)
        VALUES ($name, $email, $score, $course, $date);
        SELECT last_insert_rowid();
    ";
    cmd.Parameters.AddWithValue("$name", request.Name);
    cmd.Parameters.AddWithValue("$email", request.Email);
    cmd.Parameters.AddWithValue("$score", request.Score);
    cmd.Parameters.AddWithValue("$course", request.Course ?? "General");
    cmd.Parameters.AddWithValue("$date", DateTime.Now.ToString("yyyy-MM-dd"));

    var newId = (long)(cmd.ExecuteScalar() ?? 0);

    var newStudent = new Student(
        (int)newId,
        request.Name,
        request.Email,
        request.Score,
        request.Course ?? "General",
        DateTime.Now.ToString("yyyy-MM-dd")
    );

    return Results.Created($"/api/students/{newId}", newStudent);
})
.RequireAuthorization();

// PUT /api/students/{id} — update an existing student
app.MapPut("/api/students/{id}", (int id, UpdateStudentRequest request) =>
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    // Check the student exists first
    var checkCmd = connection.CreateCommand();
    checkCmd.CommandText = "SELECT COUNT(*) FROM Students WHERE Id = $id;";
    checkCmd.Parameters.AddWithValue("$id", id);
    var exists = (long)(checkCmd.ExecuteScalar() ?? 0) > 0;

    if (!exists)
        return Results.NotFound(new { message = $"Student with ID {id} not found." });

    var updateCmd = connection.CreateCommand();
    updateCmd.CommandText = @"
        UPDATE Students
        SET Name = $name, Email = $email, Score = $score, Course = $course
        WHERE Id = $id;
    ";
    updateCmd.Parameters.AddWithValue("$name", request.Name);
    updateCmd.Parameters.AddWithValue("$email", request.Email);
    updateCmd.Parameters.AddWithValue("$score", request.Score);
    updateCmd.Parameters.AddWithValue("$course", request.Course);
    updateCmd.Parameters.AddWithValue("$id", id);
    updateCmd.ExecuteNonQuery();

    return Results.Ok(new { message = $"Student {id} updated successfully." });
})
.RequireAuthorization();

// DELETE /api/students/{id} — delete a student
app.MapDelete("/api/students/{id}", (int id) =>
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "DELETE FROM Students WHERE Id = $id;";
    cmd.Parameters.AddWithValue("$id", id);
    int rowsAffected = cmd.ExecuteNonQuery();

    if (rowsAffected == 0)
        return Results.NotFound(new { message = $"Student with ID {id} not found." });

    return Results.Ok(new { message = $"Student {id} deleted successfully." });
})
.RequireAuthorization();

// ============================================================
// STEP 7: Dashboard stats endpoint — PROTECTED
// ============================================================
// This is the main endpoint for the dashboard. It reads all
// students from the DB and calculates useful statistics.

// GET /api/dashboard/stats
app.MapGet("/api/dashboard/stats", () =>
{
    var students = new List<(string Name, int Score)>();

    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT Name, Score FROM Students ORDER BY Score DESC;";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        students.Add((reader.GetString(0), reader.GetInt32(1)));
    }

    if (students.Count == 0)
    {
        return Results.Ok(new
        {
            totalStudents = 0,
            averageScore = 0.0,
            highestScore = 0,
            lowestScore = 0,
            gradeBreakdown = new { },
            topStudents = Array.Empty<object>()
        });
    }

    // Calculate statistics
    int total = students.Count;
    double average = students.Average(s => s.Score);
    int highest = students.Max(s => s.Score);
    int lowest = students.Min(s => s.Score);

    // Grade breakdown
    // A: 90-100, B: 80-89, C: 70-79, D: 60-69, F: below 60
    int gradeA = students.Count(s => s.Score >= 90);
    int gradeB = students.Count(s => s.Score >= 80 && s.Score < 90);
    int gradeC = students.Count(s => s.Score >= 70 && s.Score < 80);
    int gradeD = students.Count(s => s.Score >= 60 && s.Score < 70);
    int gradeF = students.Count(s => s.Score < 60);

    // Top 3 students (already sorted by score descending)
    var top3 = students
        .Take(3)
        .Select((s, index) => new { rank = index + 1, name = s.Name, score = s.Score })
        .ToList();

    return Results.Ok(new
    {
        totalStudents = total,
        averageScore = Math.Round(average, 1),
        highestScore = highest,
        lowestScore = lowest,
        gradeBreakdown = new
        {
            A = gradeA,
            B = gradeB,
            C = gradeC,
            D = gradeD,
            F = gradeF
        },
        topStudents = top3
    });
})
.RequireAuthorization();

// ============================================================
// STEP 8: Start the server
// ============================================================

Console.WriteLine("==============================================");
Console.WriteLine("  Lesson 36 API — Dashboard with JWT Auth");
Console.WriteLine("==============================================");
Console.WriteLine("  Login:  POST  http://localhost:5036/api/auth/login");
Console.WriteLine("  Stats:  GET   http://localhost:5036/api/dashboard/stats");
Console.WriteLine("  List:   GET   http://localhost:5036/api/students");
Console.WriteLine("  Creds:  admin / password123");
Console.WriteLine("==============================================");

app.Run("http://localhost:5036");

// ============================================================
// Record Types — used for request/response shapes
// ============================================================

record Student(int Id, string Name, string Email, int Score, string Course, string EnrolledDate);
record LoginRequest(string Username, string Password);
record CreateStudentRequest(string Name, string Email, int Score, string? Course);
record UpdateStudentRequest(string Name, string Email, int Score, string Course);
