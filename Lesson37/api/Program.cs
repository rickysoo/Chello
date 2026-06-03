// ============================================
// LESSON 37: Full CRUD API
// Add, Edit, Delete items via ASP.NET Web API
// ============================================
//
// CRUD = Create, Read, Update, Delete — the four fundamental
// operations every data-driven application needs.
//
// This lesson wires all four operations to a SQL Server table
// and exposes them as a REST API consumed by the frontend.
//
// ENDPOINTS:
//   GET    /api/students          → return all students
//   GET    /api/students/{id}     → return one student
//   POST   /api/students          → add a new student
//   PUT    /api/students/{id}     → update an existing student
//   DELETE /api/students/{id}     → remove a student
//
// SQL TABLE (run once to set up):
//
//   CREATE TABLE Students (
//       Id    INT IDENTITY(1,1) PRIMARY KEY,
//       Name  NVARCHAR(100) NOT NULL,
//       Email NVARCHAR(100) NOT NULL,
//       Score INT NOT NULL DEFAULT 0
//   );
//
//   INSERT INTO Students (Name, Email, Score) VALUES
//       ('Alice Wong',   'alice@example.com',   92),
//       ('Bob Tan',      'bob@example.com',     78),
//       ('Charlie Lim',  'charlie@example.com', 85),
//       ('Diana Cruz',   'diana@example.com',   67);
//
// HOW TO RUN:
//   cd Lesson37/api
//   dotnet run
//   API listens on http://localhost:5000

using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CORS — allow the frontend (opened as a local
// HTML file) to call this API without browser
// security blocking the request.
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// ============================================
// Database connection string
// Update "Server" and "Database" to match your
// SQL Server instance.
// ============================================
const string ConnectionString =
    "Server=localhost;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";


// ============================================
// HELPER — grade from score
// Keeps the grading logic in one place so we
// don't repeat it across multiple endpoints.
// ============================================
static string GetGrade(int score) => score switch
{
    >= 90 => "A",
    >= 80 => "B",
    >= 70 => "C",
    >= 60 => "D",
    _     => "F"
};


// ============================================
// GET /api/students
// Return every row in the Students table.
// ============================================
app.MapGet("/api/students", async () =>
{
    var students = new List<object>();

    using var conn = new SqlConnection(ConnectionString);
    await conn.OpenAsync();

    // Select all columns — ORDER BY Id so the list is stable
    var sql = "SELECT Id, Name, Email, Score FROM Students ORDER BY Id";
    using var cmd  = new SqlCommand(sql, conn);
    using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        int score = reader.GetInt32(3);
        students.Add(new
        {
            Id    = reader.GetInt32(0),
            Name  = reader.GetString(1),
            Email = reader.GetString(2),
            Score = score,
            Grade = GetGrade(score)   // computed on the fly — not stored in DB
        });
    }

    return Results.Ok(students);
});


// ============================================
// GET /api/students/{id}
// Return a single student by primary key.
// Returns 404 if not found.
// ============================================
app.MapGet("/api/students/{id:int}", async (int id) =>
{
    using var conn = new SqlConnection(ConnectionString);
    await conn.OpenAsync();

    var sql = "SELECT Id, Name, Email, Score FROM Students WHERE Id = @Id";
    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@Id", id);   // parameterized — prevents SQL injection

    using var reader = await cmd.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
        return Results.NotFound(new { Message = $"Student {id} not found." });

    int score = reader.GetInt32(3);
    var student = new
    {
        Id    = reader.GetInt32(0),
        Name  = reader.GetString(1),
        Email = reader.GetString(2),
        Score = score,
        Grade = GetGrade(score)
    };

    return Results.Ok(student);
});


// ============================================
// POST /api/students
// Insert a new student row.
// Returns 201 Created with the new student
// (including the auto-generated Id).
// ============================================
app.MapPost("/api/students", async (StudentRequest req) =>
{
    // Basic validation — return 400 Bad Request if data is missing
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email))
        return Results.BadRequest(new { Message = "Name and Email are required." });

    if (req.Score < 0 || req.Score > 100)
        return Results.BadRequest(new { Message = "Score must be between 0 and 100." });

    using var conn = new SqlConnection(ConnectionString);
    await conn.OpenAsync();

    // OUTPUT INSERTED.Id tells SQL Server to return the new auto-generated Id
    var sql = @"
        INSERT INTO Students (Name, Email, Score)
        OUTPUT INSERTED.Id
        VALUES (@Name, @Email, @Score)";

    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@Name",  req.Name.Trim());
    cmd.Parameters.AddWithValue("@Email", req.Email.Trim());
    cmd.Parameters.AddWithValue("@Score", req.Score);

    // ExecuteScalarAsync returns the single value produced by OUTPUT
    int newId = (int)(await cmd.ExecuteScalarAsync())!;

    var created = new
    {
        Id    = newId,
        Name  = req.Name.Trim(),
        Email = req.Email.Trim(),
        Score = req.Score,
        Grade = GetGrade(req.Score)
    };

    // 201 Created — standard HTTP status for a successful insertion
    return Results.Created($"/api/students/{newId}", created);
});


// ============================================
// PUT /api/students/{id}
// Replace all editable fields for one student.
// Returns 200 with the updated student,
// or 404 if the Id does not exist.
// ============================================
app.MapPut("/api/students/{id:int}", async (int id, StudentRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email))
        return Results.BadRequest(new { Message = "Name and Email are required." });

    if (req.Score < 0 || req.Score > 100)
        return Results.BadRequest(new { Message = "Score must be between 0 and 100." });

    using var conn = new SqlConnection(ConnectionString);
    await conn.OpenAsync();

    var sql = @"
        UPDATE Students
        SET Name  = @Name,
            Email = @Email,
            Score = @Score
        WHERE Id = @Id";

    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@Id",    id);
    cmd.Parameters.AddWithValue("@Name",  req.Name.Trim());
    cmd.Parameters.AddWithValue("@Email", req.Email.Trim());
    cmd.Parameters.AddWithValue("@Score", req.Score);

    // ExecuteNonQueryAsync returns the number of rows affected
    int rowsAffected = await cmd.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
        return Results.NotFound(new { Message = $"Student {id} not found." });

    var updated = new
    {
        Id    = id,
        Name  = req.Name.Trim(),
        Email = req.Email.Trim(),
        Score = req.Score,
        Grade = GetGrade(req.Score)
    };

    return Results.Ok(updated);
});


// ============================================
// DELETE /api/students/{id}
// Remove a student row permanently.
// Returns 200 on success, 404 if not found.
// ============================================
app.MapDelete("/api/students/{id:int}", async (int id) =>
{
    using var conn = new SqlConnection(ConnectionString);
    await conn.OpenAsync();

    var sql = "DELETE FROM Students WHERE Id = @Id";
    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@Id", id);

    int rowsAffected = await cmd.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
        return Results.NotFound(new { Message = $"Student {id} not found." });

    return Results.Ok(new { Message = $"Student {id} deleted successfully." });
});


app.Run();


// ============================================
// MODELS
// ============================================

// StudentRequest is used for both POST (create) and PUT (update) bodies.
// We use one shared class because both operations accept the same fields.
class StudentRequest
{
    public string Name  { get; set; } = "";
    public string Email { get; set; } = "";
    public int    Score { get; set; }
}
