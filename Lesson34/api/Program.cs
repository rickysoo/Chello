// ============================================================
// Lesson 34: User Registration — Sign up and save to DB
// ASP.NET Minimal Web API
// ============================================================
// What this file does:
//   1. Starts a web server on http://localhost:5034
//   2. Exposes POST /api/auth/register  — create a new account
//   3. Exposes GET  /api/users          — list all registered users
//
// Run this API with:
//   cd Lesson34/api
//   dotnet run
// ============================================================

using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CORS — Cross-Origin Resource Sharing
// ============================================================
// Our frontend HTML file runs from a different "origin" than our API.
// Browsers block cross-origin requests by default for security.
// We must tell the API: "it's okay to accept requests from the frontend."
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()   // Accept requests from any origin (fine for dev)
            .AllowAnyMethod()   // Allow GET, POST, PUT, DELETE, etc.
            .AllowAnyHeader();  // Allow any request headers
    });
});

var app = builder.Build();

// Apply the CORS policy to every request
app.UseCors("AllowFrontend");

// ============================================================
// DATABASE CONNECTION STRING
// ============================================================
// This tells the app how to connect to SQL Server.
// Update "Server" if your SQL Server has a different name.
// ============================================================
const string ConnectionString =
    "Server=localhost;Database=ChelloApp;Trusted_Connection=True;TrustServerCertificate=True;";

// ============================================================
// HELPER: Hash a password using SHA-256
// ============================================================
// We NEVER store passwords as plain text — that would be a
// serious security risk. If the database is ever leaked,
// all user passwords would be exposed.
//
// Instead, we "hash" the password:
//   - Hashing is a one-way transformation
//   - "hello123" always produces the same hash
//   - You cannot reverse a hash back to the original password
//   - To verify login: hash the input and compare with stored hash
//
// NOTE FOR PRODUCTION:
//   SHA-256 is acceptable here for learning, but in a real app
//   you should use BCrypt, Argon2, or PBKDF2.
//   These are designed specifically for passwords — they are slow
//   on purpose, making brute-force attacks much harder.
//   Install BCrypt.Net-Next NuGet package to use BCrypt.
// ============================================================
static string HashPassword(string password)
{
    // Convert the password string into bytes
    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

    // Run SHA-256 on those bytes
    byte[] hashBytes = SHA256.HashData(passwordBytes);

    // Convert the byte array to a readable hex string
    // e.g. "a665a45920422f9d417e4867efdc4fb8..."
    return Convert.ToHexString(hashBytes).ToLower();
}

// ============================================================
// HELPER: Validate email format
// ============================================================
// We use a simple regex (regular expression) to check that the
// email looks valid — i.e. it has an @ sign and a domain part.
// This does NOT guarantee the email exists, just that it is
// formatted like a real email address.
// ============================================================
static bool IsValidEmail(string email)
{
    // Regex pattern: something @ something . something
    var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
}

// ============================================================
// ENDPOINT: POST /api/auth/register
// ============================================================
// The frontend sends a JSON body like:
// {
//   "username": "ricky",
//   "email": "ricky@example.com",
//   "password": "mypassword"
// }
//
// Steps:
//   1. Read and validate the incoming data
//   2. Hash the password
//   3. Insert the new user into the database
//   4. Return the new user's ID
// ============================================================
app.MapPost("/api/auth/register", async (RegisterRequest request) =>
{
    // ---- Step 1: Validate input ----

    // Check that all fields were provided (not null or empty whitespace)
    if (string.IsNullOrWhiteSpace(request.Username))
        return Results.BadRequest(new { error = "Username is required." });

    if (string.IsNullOrWhiteSpace(request.Email))
        return Results.BadRequest(new { error = "Email is required." });

    if (string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { error = "Password is required." });

    // Username: trim extra spaces, enforce a length limit
    var username = request.Username.Trim();
    if (username.Length < 3 || username.Length > 50)
        return Results.BadRequest(new { error = "Username must be between 3 and 50 characters." });

    // Email: must be a valid format
    var email = request.Email.Trim().ToLower();
    if (!IsValidEmail(email))
        return Results.BadRequest(new { error = "Please enter a valid email address." });

    // Password: minimum 6 characters (very basic — enforce stronger rules in production)
    if (request.Password.Length < 6)
        return Results.BadRequest(new { error = "Password must be at least 6 characters long." });

    // ---- Step 2: Hash the password ----
    var passwordHash = HashPassword(request.Password);

    // ---- Step 3: Save to database ----
    try
    {
        // Open a connection to SQL Server
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        // SQL command to insert a new row into the Users table
        // We use @Username, @Email, @PasswordHash as placeholders (parameters)
        // This prevents SQL Injection attacks — never concatenate user input into SQL!
        var sql = @"
            INSERT INTO Users (Username, Email, PasswordHash)
            OUTPUT INSERTED.Id
            VALUES (@Username, @Email, @PasswordHash)";

        using var command = new SqlCommand(sql, connection);

        // Bind the C# values to the SQL parameter placeholders
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);

        // ExecuteScalarAsync returns the first column of the first row
        // Because of OUTPUT INSERTED.Id, this will be the new user's Id
        var newUserId = await command.ExecuteScalarAsync();

        // Return HTTP 201 Created with a success message
        return Results.Created($"/api/users/{newUserId}", new
        {
            message = "Registration successful! Welcome aboard.",
            userId = newUserId
        });
    }
    catch (SqlException sqlEx)
    {
        // SQL Server error number 2627 = unique constraint violation
        // This happens if the username or email is already taken
        if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
        {
            // Check the error message to give a more helpful response
            if (sqlEx.Message.Contains("Username"))
                return Results.Conflict(new { error = "That username is already taken. Please choose another." });

            if (sqlEx.Message.Contains("Email"))
                return Results.Conflict(new { error = "An account with that email already exists." });

            return Results.Conflict(new { error = "Username or email is already registered." });
        }

        // Any other database error — log it and return a generic message
        Console.WriteLine($"[DB ERROR] {sqlEx.Message}");
        return Results.Problem("A database error occurred. Please try again.");
    }
    catch (Exception ex)
    {
        // Catch-all for unexpected errors
        Console.WriteLine($"[ERROR] {ex.Message}");
        return Results.Problem("An unexpected error occurred. Please try again.");
    }
});

// ============================================================
// ENDPOINT: GET /api/users
// ============================================================
// Returns a list of all registered users.
// IMPORTANT: We do NOT return the PasswordHash column.
//   - Never expose password hashes to the frontend.
//   - Even hashes should be kept private.
// ============================================================
app.MapGet("/api/users", async () =>
{
    try
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Select all columns EXCEPT PasswordHash
        var sql = "SELECT Id, Username, Email, CreatedAt FROM Users ORDER BY CreatedAt DESC";
        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        // Read each row and build a list of user objects
        var users = new List<object>();
        while (await reader.ReadAsync())
        {
            users.Add(new
            {
                id = reader.GetInt32(0),
                username = reader.GetString(1),
                email = reader.GetString(2),
                createdAt = reader.GetDateTime(3)
            });
        }

        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");
        return Results.Problem("Could not retrieve users.");
    }
});

// ============================================================
// Start the web server
// ============================================================
Console.WriteLine("Lesson 34 API running at http://localhost:5034");
Console.WriteLine("Endpoints:");
Console.WriteLine("  POST http://localhost:5034/api/auth/register");
Console.WriteLine("  GET  http://localhost:5034/api/users");

app.Run("http://localhost:5034");

// ============================================================
// REQUEST MODEL
// ============================================================
// This is a record — a simple class that holds data.
// ASP.NET automatically maps the incoming JSON body to this record.
// The property names must match the JSON field names (case-insensitive).
// ============================================================
record RegisterRequest(string? Username, string? Email, string? Password);
