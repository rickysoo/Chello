// ============================================
// LESSON 35: User Login — JWT Authentication
// Register, log in, and protect endpoints with tokens
// ============================================
//
// In this lesson we connect everything we know:
//   • A SQL database to store real users
//   • Password hashing so plain-text passwords are never saved
//   • JWT tokens so the client can prove identity on every request
//
// FLOW:
//   1. POST /api/auth/register  → save username + hashed password to DB
//   2. POST /api/auth/login     → check credentials, return a JWT token
//   3. GET  /api/profile        → protected; reads user info from the token
//
// ============================================
// DATABASE SETUP (run this in SQL Server first):
// ============================================
//
//   CREATE DATABASE Lesson35;
//   GO
//   USE Lesson35;
//   GO
//   CREATE TABLE Users (
//       Id           INT IDENTITY(1,1) PRIMARY KEY,
//       Username     NVARCHAR(100) NOT NULL UNIQUE,
//       PasswordHash NVARCHAR(256) NOT NULL,
//       CreatedAt    DATETIME2     NOT NULL DEFAULT GETUTCDATE()
//   );
//
// ============================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

// ── Configuration ──────────────────────────────────────────────────────────────
//
// IMPORTANT: In a real app, store the JWT secret and connection string in
// environment variables or a secrets manager — NEVER hard-code them in source.
// They are hard-coded here only for educational clarity.

const string JwtSecret     = "chello-lesson35-secret-key-must-be-at-least-32-chars";
const string JwtIssuer     = "Lesson35App";
const string ConnectionStr = "Server=localhost;Database=Lesson35;Trusted_Connection=True;TrustServerCertificate=True;";

// ── Builder ────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Allow the React frontend (served from a different origin) to call this API.
// CORS = Cross-Origin Resource Sharing — browsers block cross-origin requests by
// default; this tells the browser "it's OK, we allow it".
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500", "null")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register JWT authentication — the middleware will read the "Authorization: Bearer ..."
// header on every request, verify the token signature, and populate HttpContext.User.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = false,   // we skip audience for simplicity
            ValidateLifetime         = true,    // reject expired tokens
            ValidateIssuerSigningKey = true,
            ValidIssuer              = JwtIssuer,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseAuthentication();   // must come BEFORE UseAuthorization
app.UseAuthorization();


// ============================================
// POST /api/auth/register
// PUBLIC — no token needed
// Body: { "username": "alice", "password": "mypassword" }
// ============================================
//
// We NEVER store the raw password.
// Instead we hash it with SHA-256 and store the hash.
// On login we hash the supplied password and compare hashes.

app.MapPost("/api/auth/register", async (RegisterRequest req) =>
{
    // Basic validation
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { Error = "Username and password are required." });

    if (req.Password.Length < 6)
        return Results.BadRequest(new { Error = "Password must be at least 6 characters." });

    // Hash the password before saving to the database
    string passwordHash = HashPassword(req.Password);

    try
    {
        using var conn = new SqlConnection(ConnectionStr);
        await conn.OpenAsync();

        // Check if username already exists
        using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @Username", conn);
        checkCmd.Parameters.AddWithValue("@Username", req.Username);
        int count = (int)await checkCmd.ExecuteScalarAsync()!;

        if (count > 0)
            return Results.Conflict(new { Error = "Username is already taken." });

        // Insert the new user — note we store PasswordHash, NOT the plain password
        using var insertCmd = new SqlCommand(
            "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)", conn);
        insertCmd.Parameters.AddWithValue("@Username",     req.Username);
        insertCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
        await insertCmd.ExecuteNonQueryAsync();

        return Results.Ok(new { Message = $"User '{req.Username}' registered successfully." });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});


// ============================================
// POST /api/auth/login
// PUBLIC — no token needed
// Body: { "username": "alice", "password": "mypassword" }
// Returns: { "token": "...", "username": "...", "expiresIn": "1 hour" }
// ============================================

app.MapPost("/api/auth/login", async (LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { Error = "Username and password are required." });

    try
    {
        using var conn = new SqlConnection(ConnectionStr);
        await conn.OpenAsync();

        // Look up the user by username
        using var cmd = new SqlCommand(
            "SELECT Id, Username, PasswordHash FROM Users WHERE Username = @Username", conn);
        cmd.Parameters.AddWithValue("@Username", req.Username);

        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            // User not found — use the same message as wrong password to avoid
            // leaking information about which usernames exist (security best practice)
            return Results.Unauthorized();
        }

        int    userId       = reader.GetInt32(0);
        string username     = reader.GetString(1);
        string storedHash   = reader.GetString(2);

        // Hash the supplied password and compare with the stored hash
        string suppliedHash = HashPassword(req.Password);

        if (suppliedHash != storedHash)
            return Results.Unauthorized();   // wrong password

        // Credentials are correct — build a JWT token
        // ────────────────────────────────────────────
        // Claims are small pieces of information embedded inside the token.
        // The client cannot change them without invalidating the token signature.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),   // numeric user ID
            new Claim(ClaimTypes.Name,           username),             // username string
            new Claim("registered_at",           DateTime.UtcNow.ToString("o"))
        };

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             JwtIssuer,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(1),   // token expires after 1 hour
            signingCredentials: credentials
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new
        {
            Token     = tokenString,
            Username  = username,
            ExpiresIn = "1 hour"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});


// ============================================
// GET /api/profile
// PROTECTED — requires valid JWT in Authorization header
// The client must send: Authorization: Bearer <token>
// ============================================
//
// RequireAuthorization() tells ASP.NET to reject the request with
// 401 Unauthorized if no valid token is present.
//
// When the token IS valid, HttpContext.User is populated from the token claims.

app.MapGet("/api/profile", (ClaimsPrincipal user) =>
{
    // Read the claims that were embedded in the token during login
    string userId   = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
    string username = user.Identity?.Name ?? "unknown";

    return Results.Ok(new
    {
        UserId   = userId,
        Username = username,
        Message  = $"Hello, {username}! You are logged in."
    });

}).RequireAuthorization();   // ← this single line protects the endpoint


// ── Helper: password hashing ───────────────────────────────────────────────────
//
// SHA-256 is a one-way hash function.
// Given the same input it always produces the same output,
// but you cannot reverse the hash back to the original password.
// This means even if the database is stolen, the attacker cannot
// easily recover user passwords.
//
// NOTE: For production apps use BCrypt or Argon2 which are specifically
// designed for password hashing and include a salt by default.

static string HashPassword(string password)
{
    byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
    return Convert.ToHexString(bytes);   // returns uppercase hex string, e.g. "5E884898..."
}


app.Run();


// ============================================
// MODELS
// ============================================

class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
