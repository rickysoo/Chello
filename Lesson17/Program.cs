// ============================================
// LESSON 17: POST Endpoints
// Receiving and saving new data
// ============================================
//
// GET  = asking for data  (read only)
// POST = sending new data (create something new)
//
// When you submit a form or add a new record, the frontend
// sends a POST request with data in the request BODY as JSON.
//
// HOW TO TEST using REST Client in VS Code:
// Create a file called test.http and paste:
//
//   POST http://localhost:5000/api/students
//   Content-Type: application/json
//
//   {
//     "firstName": "Eve",
//     "lastName": "Ng",
//     "email": "eve@email.com",
//     "score": 75
//   }

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory list with starting data
var students = new List<Student>
{
    new Student { Id = 1, FirstName = "Ricky",   LastName = "Soo",  Email = "ricky@email.com",   Score = 95 },
    new Student { Id = 2, FirstName = "Alice",   LastName = "Wong", Email = "alice@email.com",   Score = 88 },
    new Student { Id = 3, FirstName = "Bob",     LastName = "Tan",  Email = "bob@email.com",     Score = 80 },
    new Student { Id = 4, FirstName = "Charlie", LastName = "Lim",  Email = "charlie@email.com", Score = 60 },
};

int nextId = 5;   // tracks the next available ID


// ============================================
// GET /api/students — view all (from Lesson 16)
// ============================================

app.MapGet("/api/students", () => Results.Ok(students.OrderBy(s => s.Id)));


// ============================================
// POST /api/students — add a new student
// ============================================

app.MapPost("/api/students", (StudentRequest request) =>
{
    // Validate the incoming data
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(request.FirstName))
        errors.Add("FirstName is required.");

    if (string.IsNullOrWhiteSpace(request.LastName))
        errors.Add("LastName is required.");

    if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
        errors.Add("A valid Email is required.");

    if (request.Score < 0 || request.Score > 100)
        errors.Add("Score must be between 0 and 100.");

    // If validation failed, return 400 Bad Request with the list of errors
    if (errors.Any())
        return Results.BadRequest(new { Errors = errors });

    // Create the new student
    var newStudent = new Student
    {
        Id        = nextId++,
        FirstName = request.FirstName,
        LastName  = request.LastName,
        Email     = request.Email,
        Score     = request.Score
    };

    students.Add(newStudent);

    // 201 Created — includes the URL where the new resource can be found
    return Results.Created($"/api/students/{newStudent.Id}", newStudent);
});


app.Run();


// ============================================
// Student — the full model returned by the API
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


// ============================================
// StudentRequest — what the caller sends in the POST body
// Separate from Student so callers don't set Id themselves
// ============================================

class StudentRequest
{
    public string FirstName { get; set; } = "";
    public string LastName  { get; set; } = "";
    public string Email     { get; set; } = "";
    public int    Score     { get; set; }
}
