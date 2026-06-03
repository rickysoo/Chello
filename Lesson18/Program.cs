// ============================================
// LESSON 18: PUT & DELETE Endpoints
// Update and remove records via the API
// ============================================
//
// Full HTTP method summary:
//   GET    = Read data
//   POST   = Create new data
//   PUT    = Update existing data (replace the whole record)
//   DELETE = Remove data
//
// HOW TO TEST using REST Client in VS Code (test.http):
//
//   ### Update student 2
//   PUT http://localhost:5000/api/students/2
//   Content-Type: application/json
//
//   { "firstName": "Alice", "lastName": "Wong", "email": "alice.new@email.com", "score": 92 }
//
//   ### Delete student 4
//   DELETE http://localhost:5000/api/students/4

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var students = new List<Student>
{
    new Student { Id = 1, FirstName = "Ricky",   LastName = "Soo",  Email = "ricky@email.com",   Score = 95 },
    new Student { Id = 2, FirstName = "Alice",   LastName = "Wong", Email = "alice@email.com",   Score = 88 },
    new Student { Id = 3, FirstName = "Bob",     LastName = "Tan",  Email = "bob@email.com",     Score = 80 },
    new Student { Id = 4, FirstName = "Charlie", LastName = "Lim",  Email = "charlie@email.com", Score = 60 },
};

int nextId = 5;


// ============================================
// GET all students
// ============================================

app.MapGet("/api/students", () =>
    Results.Ok(students.OrderBy(s => s.Id)));


// ============================================
// GET one student
// ============================================

app.MapGet("/api/students/{id}", (int id) =>
{
    var student = students.FirstOrDefault(s => s.Id == id);
    return student is null
        ? Results.NotFound(new { Message = $"Student {id} not found." })
        : Results.Ok(student);
});


// ============================================
// POST — create a new student
// ============================================

app.MapPost("/api/students", (StudentRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
        return Results.BadRequest(new { Message = "FirstName and LastName are required." });

    var student = new Student
    {
        Id        = nextId++,
        FirstName = req.FirstName,
        LastName  = req.LastName,
        Email     = req.Email,
        Score     = req.Score
    };

    students.Add(student);
    return Results.Created($"/api/students/{student.Id}", student);
});


// ============================================
// PUT /api/students/{id} — update whole record
// ============================================

app.MapPut("/api/students/{id}", (int id, StudentRequest req) =>
{
    var student = students.FirstOrDefault(s => s.Id == id);

    if (student is null)
        return Results.NotFound(new { Message = $"Student {id} not found." });

    // Validate
    if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
        return Results.BadRequest(new { Message = "FirstName and LastName are required." });

    if (req.Score < 0 || req.Score > 100)
        return Results.BadRequest(new { Message = "Score must be 0-100." });

    // Update all fields
    student.FirstName = req.FirstName;
    student.LastName  = req.LastName;
    student.Email     = req.Email;
    student.Score     = req.Score;

    return Results.Ok(student);
});


// ============================================
// DELETE /api/students/{id} — remove a record
// ============================================

app.MapDelete("/api/students/{id}", (int id) =>
{
    var student = students.FirstOrDefault(s => s.Id == id);

    if (student is null)
        return Results.NotFound(new { Message = $"Student {id} not found." });

    students.Remove(student);

    // 204 No Content — success but nothing to return
    return Results.NoContent();
});


app.Run();


// ============================================
// MODELS
// ============================================

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
