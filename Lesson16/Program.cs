// ============================================
// LESSON 16: GET Endpoints
// Returning data as JSON properly
// ============================================
//
// In Lesson 15 we used anonymous objects.
// In this lesson we use proper classes (models) — the professional way.
//
// We also learn:
//   - Query string parameters (?search=alice)
//   - HTTP status codes (200, 404, etc.)
//   - Organising endpoints with route prefixes
//
// HOW TO TEST (open browser or use REST Client in VS Code):
//   GET http://localhost:5000/api/students
//   GET http://localhost:5000/api/students/1
//   GET http://localhost:5000/api/students?search=alice
//   GET http://localhost:5000/api/students?minScore=80
//   GET http://localhost:5000/api/students/stats

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory list (replaces the database for now)
var students = new List<Student>
{
    new Student { Id = 1, FirstName = "Ricky",   LastName = "Soo",  Email = "ricky@email.com",   Score = 95 },
    new Student { Id = 2, FirstName = "Alice",   LastName = "Wong", Email = "alice@email.com",   Score = 88 },
    new Student { Id = 3, FirstName = "Bob",     LastName = "Tan",  Email = "bob@email.com",     Score = 80 },
    new Student { Id = 4, FirstName = "Charlie", LastName = "Lim",  Email = "charlie@email.com", Score = 60 },
};


// ============================================
// GET /api/students
// Returns all students, with optional filters
// ============================================

app.MapGet("/api/students", (string? search, int? minScore) =>
{
    var result = students.AsEnumerable();

    // Filter by name if search param provided
    // Example: /api/students?search=alice
    if (!string.IsNullOrEmpty(search))
        result = result.Where(s =>
            s.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            s.LastName.Contains(search, StringComparison.OrdinalIgnoreCase));

    // Filter by minimum score if provided
    // Example: /api/students?minScore=80
    if (minScore.HasValue)
        result = result.Where(s => s.Score >= minScore.Value);

    var list = result.OrderByDescending(s => s.Score).ToList();

    return Results.Ok(list);
});


// ============================================
// GET /api/students/{id}
// Returns one student by ID
// ============================================

app.MapGet("/api/students/{id}", (int id) =>
{
    var student = students.FirstOrDefault(s => s.Id == id);

    if (student == null)
        return Results.NotFound(new { Message = $"Student with ID {id} not found." });

    return Results.Ok(student);
});


// ============================================
// GET /api/students/stats
// Returns class statistics
// ============================================

app.MapGet("/api/students/stats", () =>
{
    if (!students.Any())
        return Results.Ok(new { Message = "No students found." });

    var stats = new
    {
        Total        = students.Count,
        AverageScore = Math.Round(students.Average(s => s.Score), 1),
        HighestScore = students.Max(s => s.Score),
        LowestScore  = students.Min(s => s.Score),
        GradeA       = students.Count(s => s.Score >= 90),
        GradeB       = students.Count(s => s.Score >= 80 && s.Score < 90),
        GradeC       = students.Count(s => s.Score >= 70 && s.Score < 80),
        GradeD       = students.Count(s => s.Score >= 60 && s.Score < 70),
        GradeF       = students.Count(s => s.Score < 60),
    };

    return Results.Ok(stats);
});


app.Run();


// ============================================
// MODEL: Student class
// A class used to represent data sent/received by the API
// ============================================

class Student
{
    public int    Id        { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName  { get; set; } = "";
    public string Email     { get; set; } = "";
    public int    Score     { get; set; }

    // Computed property — calculated automatically, not stored
    public string Grade => Score switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _     => "F"
    };
}
