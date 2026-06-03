// ============================================
// LESSON 15: What is an API?
// Your first ASP.NET Web API
// ============================================
//
// API = Application Programming Interface
// A Web API is a program that:
//   - Runs on a server and listens for requests
//   - Receives requests from browsers, apps, or other programs
//   - Returns data as JSON (a text format everyone understands)
//
// Think of it like a waiter at a restaurant:
//   You (React app) → ask waiter (API) → kitchen (database) → waiter returns food (data)
//
// HOW TO TEST:
//   1. Run this program (dotnet run)
//   2. Open your browser and go to:
//      http://localhost:5000/hello
//      http://localhost:5000/about
//      http://localhost:5000/time
//      http://localhost:5000/students

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ============================================
// ENDPOINT 1: Simple hello message
// URL: GET http://localhost:5000/hello
// ============================================

app.MapGet("/hello", () =>
{
    // Returns plain text
    return "Hello from your first API!";
});

// ============================================
// ENDPOINT 2: Return an object as JSON
// URL: GET http://localhost:5000/about
// ============================================

app.MapGet("/about", () =>
{
    // Returns an object — ASP.NET automatically converts it to JSON
    return new
    {
        CourseName = "Chello",
        Description = "Learning C# from Hello World to Full Stack",
        CurrentLesson = 15,
        Author = "Ricky"
    };
});

// ============================================
// ENDPOINT 3: Return the current time
// URL: GET http://localhost:5000/time
// ============================================

app.MapGet("/time", () =>
{
    return new
    {
        CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        Message = "This time came from your C# server!"
    };
});

// ============================================
// ENDPOINT 4: Return a list as JSON
// URL: GET http://localhost:5000/students
// ============================================

app.MapGet("/students", () =>
{
    // A hardcoded list for now — we connect to a real DB in Lesson 19
    var students = new[]
    {
        new { Id = 1, Name = "Ricky Soo",    Score = 95 },
        new { Id = 2, Name = "Alice Wong",   Score = 88 },
        new { Id = 3, Name = "Bob Tan",      Score = 80 },
        new { Id = 4, Name = "Charlie Lim",  Score = 60 },
    };

    return students;
});

// ============================================
// ENDPOINT 5: URL parameter — get one student
// URL: GET http://localhost:5000/students/2
// ============================================

app.MapGet("/students/{id}", (int id) =>
{
    var students = new[]
    {
        new { Id = 1, Name = "Ricky Soo",   Score = 95 },
        new { Id = 2, Name = "Alice Wong",  Score = 88 },
        new { Id = 3, Name = "Bob Tan",     Score = 80 },
        new { Id = 4, Name = "Charlie Lim", Score = 60 },
    };

    var student = Array.Find(students, s => s.Id == id);

    if (student == null)
        return Results.NotFound(new { Message = "Student not found", Id = id });

    return Results.Ok(student);
});

app.Run();
