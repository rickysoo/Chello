// ============================================================
// LESSON 38: Search & Filter — Live Search with DB Queries
// ============================================================
// This ASP.NET Web API demonstrates how to build a flexible
// search endpoint that accepts multiple query parameters and
// converts them into a single SQL query with proper filtering,
// sorting, and pagination — all done safely in the database.
//
// Key Concepts:
//   - Query parameters: search, minScore, maxScore, grade, sortBy, sortOrder, page, pageSize
//   - Dynamic SQL building with parameterized queries (no SQL injection)
//   - Pagination: LIMIT and OFFSET in SQL
//   - CORS: allowing the frontend (different port) to call this API
// ============================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CORS SETUP
// CORS = Cross-Origin Resource Sharing
// Without this, the browser blocks requests from a different
// port (e.g., frontend on port 5500 calling API on port 5038).
// We explicitly allow all origins for this lesson demo.
// ============================================================
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

// ============================================================
// SQLITE DATABASE SETUP
// We create an in-memory SQLite database and seed it with
// sample student records so the lesson is self-contained
// (no external database server needed).
// ============================================================
const string ConnectionString = "Data Source=students.db";

void SetupDatabase()
{
    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    // Create the students table if it does not already exist
    var createTable = connection.CreateCommand();
    createTable.CommandText = """
        CREATE TABLE IF NOT EXISTS Students (
            Id      INTEGER PRIMARY KEY AUTOINCREMENT,
            Name    TEXT    NOT NULL,
            Email   TEXT    NOT NULL,
            Score   INTEGER NOT NULL,   -- score out of 100
            Grade   TEXT    NOT NULL    -- A, B, C, D, or F
        );
        """;
    createTable.ExecuteNonQuery();

    // Only seed data when the table is empty
    var countCmd = connection.CreateCommand();
    countCmd.CommandText = "SELECT COUNT(*) FROM Students;";
    long count = (long)(countCmd.ExecuteScalar() ?? 0L);

    if (count == 0)
    {
        // Insert 25 sample students so pagination is visible
        var students = new (string Name, string Email, int Score)[]
        {
            ("Alice Johnson",    "alice@example.com",    95),
            ("Bob Smith",        "bob@example.com",      82),
            ("Carol White",      "carol@example.com",    74),
            ("David Brown",      "david@example.com",    61),
            ("Eve Davis",        "eve@example.com",      88),
            ("Frank Miller",     "frank@example.com",    55),
            ("Grace Wilson",     "grace@example.com",    91),
            ("Henry Moore",      "henry@example.com",    47),
            ("Isla Taylor",      "isla@example.com",     78),
            ("Jack Anderson",    "jack@example.com",     66),
            ("Karen Thomas",     "karen@example.com",    83),
            ("Liam Jackson",     "liam@example.com",     39),
            ("Mia Harris",       "mia@example.com",      97),
            ("Noah Martin",      "noah@example.com",     71),
            ("Olivia Garcia",    "olivia@example.com",   85),
            ("Peter Martinez",   "peter@example.com",    58),
            ("Quinn Robinson",   "quinn@example.com",    92),
            ("Rachel Clark",     "rachel@example.com",   44),
            ("Sam Lewis",        "sam@example.com",      76),
            ("Tina Walker",      "tina@example.com",     89),
            ("Uma Hall",         "uma@example.com",      63),
            ("Victor Allen",     "victor@example.com",   72),
            ("Wendy Young",      "wendy@example.com",    50),
            ("Xander King",      "xander@example.com",   80),
            ("Yara Wright",      "yara@example.com",     93),
        };

        // Assign grades based on score
        static string ToGrade(int score) => score switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _     => "F"
        };

        var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO Students (Name, Email, Score, Grade) VALUES ($name, $email, $score, $grade);";
        var pName  = insert.Parameters.Add("$name",  SqliteType.Text);
        var pEmail = insert.Parameters.Add("$email", SqliteType.Text);
        var pScore = insert.Parameters.Add("$score", SqliteType.Integer);
        var pGrade = insert.Parameters.Add("$grade", SqliteType.Text);

        foreach (var (name, email, score) in students)
        {
            pName.Value  = name;
            pEmail.Value = email;
            pScore.Value = score;
            pGrade.Value = ToGrade(score);
            insert.ExecuteNonQuery();
        }

        Console.WriteLine($"Seeded {students.Length} students into the database.");
    }
}

SetupDatabase();

// ============================================================
// GET /api/students
// Supported query parameters:
//   search    — partial match on Name OR Email (case-insensitive)
//   minScore  — include only students with Score >= minScore
//   maxScore  — include only students with Score <= maxScore
//   grade     — comma-separated list of grades, e.g. "A,B"
//   sortBy    — "name" (default) or "score"
//   sortOrder — "asc" (default) or "desc"
//   page      — page number, 1-based (default 1)
//   pageSize  — records per page (default 10, max 50)
//
// Response JSON:
//   { data: [...], total: 25, page: 1, pageSize: 10, totalPages: 3 }
// ============================================================
app.MapGet("/api/students", (HttpRequest request) =>
{
    // --- Read query parameters from the URL ---
    string search    = request.Query["search"].FirstOrDefault()    ?? "";
    string sortBy    = request.Query["sortBy"].FirstOrDefault()    ?? "name";
    string sortOrder = request.Query["sortOrder"].FirstOrDefault() ?? "asc";
    string gradeList = request.Query["grade"].FirstOrDefault()     ?? "";

    int.TryParse(request.Query["minScore"].FirstOrDefault(),  out int minScore);
    int.TryParse(request.Query["maxScore"].FirstOrDefault(),  out int maxScore);
    int.TryParse(request.Query["page"].FirstOrDefault(),      out int page);
    int.TryParse(request.Query["pageSize"].FirstOrDefault(),  out int pageSize);

    // Apply sensible defaults and limits
    if (maxScore == 0) maxScore = 100;
    if (page     <= 0) page     = 1;
    if (pageSize <= 0) pageSize = 10;
    if (pageSize > 50) pageSize = 50;

    // --------------------------------------------------------
    // DYNAMIC SQL BUILDING
    // We start with a base WHERE clause and append conditions
    // only when the caller actually supplies a filter.
    // All user-supplied values go through parameters ($x) so
    // SQL injection is impossible.
    // --------------------------------------------------------
    var conditions = new List<string>();
    var parameters = new Dictionary<string, object>();

    // Search: partial match on Name OR Email
    if (!string.IsNullOrWhiteSpace(search))
    {
        conditions.Add("(Name LIKE $search OR Email LIKE $search)");
        parameters["$search"] = $"%{search}%";
    }

    // Score range
    conditions.Add("Score >= $minScore");
    parameters["$minScore"] = minScore;

    conditions.Add("Score <= $maxScore");
    parameters["$maxScore"] = maxScore;

    // Grade filter — only add when at least one grade is selected
    var grades = gradeList
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(g => new[] { "A", "B", "C", "D", "F" }.Contains(g.ToUpper()))
        .Select(g => g.ToUpper())
        .ToArray();

    if (grades.Length > 0)
    {
        // Build: Grade IN ($g0, $g1, ...)
        var placeholders = grades.Select((_, i) => $"$g{i}").ToArray();
        conditions.Add($"Grade IN ({string.Join(", ", placeholders)})");
        for (int i = 0; i < grades.Length; i++)
            parameters[$"$g{i}"] = grades[i];
    }

    string whereClause = conditions.Count > 0
        ? "WHERE " + string.Join(" AND ", conditions)
        : "";

    // --------------------------------------------------------
    // SORTING
    // We only allow specific column names to prevent injection
    // via the ORDER BY clause (parameters cannot be used there).
    // --------------------------------------------------------
    string orderColumn = sortBy.ToLower() == "score" ? "Score" : "Name";
    string orderDir    = sortOrder.ToLower() == "desc" ? "DESC" : "ASC";

    // --------------------------------------------------------
    // PAGINATION
    // LIMIT  = how many rows to return
    // OFFSET = how many rows to skip (page 1 skips 0, page 2 skips pageSize, etc.)
    // --------------------------------------------------------
    int offset = (page - 1) * pageSize;

    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    // --- Count total matching rows (for the pagination metadata) ---
    var countCmd = connection.CreateCommand();
    countCmd.CommandText = $"SELECT COUNT(*) FROM Students {whereClause};";
    foreach (var (key, value) in parameters)
        countCmd.Parameters.AddWithValue(key, value);
    long total = (long)(countCmd.ExecuteScalar() ?? 0L);

    // --- Fetch the current page of data ---
    var dataCmd = connection.CreateCommand();
    dataCmd.CommandText = $"""
        SELECT Id, Name, Email, Score, Grade
        FROM Students
        {whereClause}
        ORDER BY {orderColumn} {orderDir}
        LIMIT $limit OFFSET $offset;
        """;
    foreach (var (key, value) in parameters)
        dataCmd.Parameters.AddWithValue(key, value);
    dataCmd.Parameters.AddWithValue("$limit",  pageSize);
    dataCmd.Parameters.AddWithValue("$offset", offset);

    var students = new List<Student>();
    using var reader = dataCmd.ExecuteReader();
    while (reader.Read())
    {
        students.Add(new Student(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetInt32(3),
            reader.GetString(4)
        ));
    }

    // Calculate total pages (ceiling division)
    int totalPages = total == 0 ? 1 : (int)Math.Ceiling((double)total / pageSize);

    var response = new SearchResponse(
        Data:       students,
        Total:      (int)total,
        Page:       page,
        PageSize:   pageSize,
        TotalPages: totalPages
    );

    return Results.Ok(response);
});

Console.WriteLine("Lesson 38 API running — http://localhost:5038");
Console.WriteLine("Try: GET http://localhost:5038/api/students?search=ali&minScore=80&sortBy=score&sortOrder=desc&page=1&pageSize=5");
Console.WriteLine("Press Ctrl+C to stop.");

app.Run("http://localhost:5038");

// ============================================================
// DATA MODELS
// Record types are a concise way to define immutable data
// containers. The JSON serializer turns them into lowercase
// camelCase keys automatically (studentId -> studentId).
// ============================================================

// A single student row returned in the response
record Student(
    int    Id,
    string Name,
    string Email,
    int    Score,
    string Grade
);

// The wrapper that includes pagination metadata
record SearchResponse(
    List<Student> Data,
    int           Total,
    int           Page,
    int           PageSize,
    int           TotalPages
);
