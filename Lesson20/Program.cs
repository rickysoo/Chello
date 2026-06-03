// ============================================
// LESSON 20: API Security
// Protecting endpoints with JWT tokens
// ============================================
//
// Without security, ANYONE on the internet can call your API
// and add, edit, or delete data.
//
// JWT = JSON Web Token — a small digital "pass" the server issues
// after login. The client sends it with every request to prove identity.
//
// Flow:
//   1. Client sends username + password to POST /auth/login
//   2. Server checks credentials → returns a JWT token
//   3. Client sends token in the Authorization header on protected requests
//   4. Server checks the token → allows or denies access
//
// HOW TO TEST (REST Client):
//
//   ### 1. Login (get a token)
//   POST http://localhost:5000/auth/login
//   Content-Type: application/json
//   { "username": "admin", "password": "password123" }
//
//   ### 2. Use the token (copy from step 1)
//   GET http://localhost:5000/api/students
//   Authorization: Bearer YOUR_TOKEN_HERE
//
//   ### 3. Try without token — should get 401 Unauthorized
//   GET http://localhost:5000/api/students

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Secret key used to sign tokens — in production store this in environment variables, never in code!
const string JwtSecret = "chello-super-secret-key-must-be-at-least-32-chars";
const string JwtIssuer  = "ChelloApp";

var builder = WebApplication.CreateBuilder(args);

// Register JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = false,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = JwtIssuer,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();   // must come before UseAuthorization
app.UseAuthorization();


// ============================================
// POST /auth/login — public endpoint, no token needed
// Returns a JWT token on successful login
// ============================================

app.MapPost("/auth/login", (LoginRequest req) =>
{
    // Hardcoded users for this lesson — in production check against a Users table in the DB
    var validUsers = new Dictionary<string, string>
    {
        { "admin", "password123" },
        { "ricky", "mypassword" }
    };

    if (!validUsers.TryGetValue(req.Username, out var correctPassword) ||
        correctPassword != req.Password)
    {
        return Results.Unauthorized();
    }

    // Build the token
    var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Claims are small pieces of info stored inside the token
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, req.Username),
        new Claim(ClaimTypes.Role, req.Username == "admin" ? "Admin" : "User")
    };

    var token = new JwtSecurityToken(
        issuer:             JwtIssuer,
        claims:             claims,
        expires:            DateTime.UtcNow.AddHours(1),   // token expires in 1 hour
        signingCredentials: credentials
    );

    string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new
    {
        Token     = tokenString,
        ExpiresIn = "1 hour",
        Username  = req.Username
    });
});


// ============================================
// GET /api/students — PROTECTED: requires token
// ============================================

app.MapGet("/api/students", (ClaimsPrincipal user) =>
{
    var students = new[]
    {
        new { Id = 1, Name = "Ricky Soo",   Score = 95 },
        new { Id = 2, Name = "Alice Wong",  Score = 88 },
        new { Id = 3, Name = "Bob Tan",     Score = 80 },
        new { Id = 4, Name = "Charlie Lim", Score = 60 },
    };

    // We can read the caller's identity from the token
    string callerName = user.Identity?.Name ?? "Unknown";

    return Results.Ok(new
    {
        RequestedBy = callerName,
        Students    = students
    });

}).RequireAuthorization();   // ← this one line protects the endpoint


// ============================================
// GET /api/admin — PROTECTED: Admin role only
// ============================================

app.MapGet("/api/admin", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        Message = "Welcome to the admin panel!",
        User    = user.Identity?.Name,
        Role    = user.FindFirst(ClaimTypes.Role)?.Value
    });

}).RequireAuthorization(policy => policy.RequireRole("Admin"));


// ============================================
// GET /api/public — NO token required
// ============================================

app.MapGet("/api/public", () =>
{
    return Results.Ok(new { Message = "This endpoint is public — no token needed." });
});


app.Run();


// ============================================
// MODELS
// ============================================

class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
