# Lesson 39 — Deploy Backend: Publish C# API to Railway

In this lesson you take the Students API from Lesson 19 and put it on the internet.
By the end you will have a live URL that any browser — or your Vercel frontend — can call.

---

## What you will learn

- How to make a C# API ready for production (no hardcoded secrets)
- How to create a free Railway account
- How to connect your GitHub repo so Railway auto-deploys on every push
- How to provision a SQL Server database on Railway
- How to set environment variables safely
- How to run the database migration (create the Students table)
- How to test your live API
- How to read Railway logs when something goes wrong

---

## Prerequisites

- Lesson 19 complete (the SQL-backed Students API)
- Your code pushed to a GitHub repository
- A free Railway account (you will create one below)

---

## Step 1 — Prepare the code

The Lesson 19 `Program.cs` had the connection string hardcoded:

```csharp
// Lesson 19 — DO NOT do this in production
string connectionString = "Server=localhost\\SQLEXPRESS;...";
```

The new `api/Program.cs` in this lesson reads it from an environment variable instead:

```csharp
string connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("DATABASE_URL is not set.");
```

It also reads the port Railway assigns at runtime:

```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
```

And it exposes a health check endpoint so Railway knows the app started successfully:

```
GET /health  →  { "status": "OK", "time": "2026-06-03T..." }
```

**Copy `Lesson39/api/Program.cs` and `Lesson39/api/Lesson39Api.csproj` into your repo**
(or just keep them in the `Lesson39/api/` folder — Railway can build from a subfolder).

---

## Step 2 — Create a Railway account

1. Go to **https://railway.com** and click **Start a New Project**.
2. Sign up with your GitHub account — this lets Railway see your repos.
3. Verify your email address if prompted.
4. Railway gives you a free Starter plan with $5 of usage per month.

---

## Step 3 — Create a new project on Railway

1. In the Railway dashboard click **+ New Project**.
2. Choose **Deploy from GitHub repo**.
3. Find your Chello repo in the list and click it.
4. Railway will scan the repo and show a service card. Click **Deploy Now**.

Railway will try to build the project immediately. It will likely fail the first time
because the database variables are not set yet — that is expected. Keep going.

---

## Step 4 — Provision a SQL Server database

Railway does not host SQL Server natively, but it does host **PostgreSQL** and **MySQL**
for free. The two options are:

### Option A — Use Railway PostgreSQL (recommended for beginners)

1. Inside your project click **+ Add Service → Database → PostgreSQL**.
2. Railway creates a database and shows you the connection variables in the
   **Variables** tab of that database service.
3. Copy the value of `DATABASE_URL` — it looks like:
   `postgresql://user:password@host:5432/railway`

> Note: If you switch to PostgreSQL you need to swap `Microsoft.Data.SqlClient`
> for `Npgsql` in the csproj and update the SQL syntax slightly (e.g. use
> `RETURNING id` instead of `SELECT SCOPE_IDENTITY()`).
> See the bonus section at the bottom of this file.

### Option B — Keep SQL Server, use an external host

Use a free-tier SQL Server from **ElephantSQL** (MySQL) or **Aiven** (PostgreSQL),
or your own Azure SQL free tier, and paste the connection string into Railway as
`DATABASE_URL`.

---

## Step 5 — Create the Students table

Once the database is running you need to create the table.
Connect to your database with a client (TablePlus, DBeaver, or the Railway
web console) and run:

```sql
CREATE TABLE Students (
    Id        INT           IDENTITY(1,1) PRIMARY KEY,  -- SQL Server
    FirstName VARCHAR(100)  NOT NULL,
    LastName  VARCHAR(100)  NOT NULL,
    Email     VARCHAR(200)  NOT NULL DEFAULT '',
    Score     INT           NOT NULL DEFAULT 0
);
```

For PostgreSQL use this instead:

```sql
CREATE TABLE Students (
    Id        SERIAL        PRIMARY KEY,
    FirstName VARCHAR(100)  NOT NULL,
    LastName  VARCHAR(100)  NOT NULL,
    Email     VARCHAR(200)  NOT NULL DEFAULT '',
    Score     INT           NOT NULL DEFAULT 0
);
```

---

## Step 6 — Set environment variables on Railway

1. In the Railway dashboard click on your **API service** (not the database).
2. Go to the **Variables** tab.
3. Add these variables one by one:

| Variable name    | Example value                                              | Why it is needed                         |
|------------------|------------------------------------------------------------|------------------------------------------|
| `DATABASE_URL`   | `Server=host;Database=railway;User Id=sa;Password=...`    | The API reads the DB connection from here |
| `ALLOWED_ORIGIN` | `https://my-app.vercel.app`                                | Tells CORS which frontend is allowed     |

4. Click **Add** after each one.
5. Railway will automatically redeploy the service after you save variables.

> **Never paste secrets into your code or commit them to GitHub.**
> Environment variables on Railway are encrypted and never exposed in logs.

---

## Step 7 — Point Railway to the right project file

Railway uses `railway.toml` (already in this lesson folder) to know what to build.
Make sure `railway.toml` is at the root of your repository, then push it to GitHub.

The relevant lines in `railway.toml`:

```toml
[build]
buildCommand = "dotnet publish Lesson39/api/Lesson39Api.csproj -c Release -o out"

[deploy]
startCommand = "dotnet out/Lesson39Api.dll"
healthcheckPath = "/health"
```

If your folder structure is different, update the path in `buildCommand`.

---

## Step 8 — Trigger a deployment

1. Push your latest code (including `railway.toml`) to GitHub:

```bash
git add .
git commit -m "Lesson 39 - production API with Railway config"
git push
```

2. Railway detects the push and starts a new deployment automatically.
3. Watch the **Build Logs** tab in Railway — you will see `dotnet publish` output.
4. When the build succeeds the **Deployments** tab shows a green checkmark.

---

## Step 9 — Get your live API URL

1. In the Railway dashboard click on your service.
2. Go to the **Settings** tab and find **Domains**.
3. Click **Generate Domain** — Railway gives you a URL like:
   `https://lesson39api-production.up.railway.app`
4. Test the health check:

```
GET https://lesson39api-production.up.railway.app/health
```

Expected response:

```json
{ "status": "OK", "time": "2026-06-03T10:00:00Z" }
```

5. Test the students endpoint:

```
GET https://lesson39api-production.up.railway.app/api/students
```

---

## Step 10 — Update the Vercel frontend

If you deployed a frontend in a previous lesson, update its API base URL to point
to the new Railway domain. In Vercel, set an environment variable:

```
NEXT_PUBLIC_API_URL = https://lesson39api-production.up.railway.app
```

Then redeploy the frontend.

---

## Common errors and fixes

### "DATABASE_URL environment variable is not set"

You forgot to add the variable in Railway. Go to **Service → Variables** and add it.

### Build fails: "project file not found"

Check the `buildCommand` in `railway.toml`. The path must match your actual folder
structure. Use `Lesson39/api/Lesson39Api.csproj` if the file is in that location.

### "Connection refused" or "Cannot open database"

The connection string is wrong or the database is not accessible from Railway.
Double-check the host, port, username, and password in `DATABASE_URL`.
For SQL Server, make sure `TrustServerCertificate=True` is in the connection string.

### CORS error in the browser

Make sure `ALLOWED_ORIGIN` is set to your exact Vercel URL including `https://`.
No trailing slash. For example: `https://my-app.vercel.app` not `https://my-app.vercel.app/`.

### Health check times out → service keeps restarting

The app is crashing on startup (usually a missing env var or a DB connection error).
Click **View Logs** in Railway to see the exact error message.

### "dotnet: command not found" during build

Railway's Nixpacks builder installs .NET automatically. If it cannot find the right
version, add a `.nixpacks.toml` file at the repo root:

```toml
[phases.setup]
nixPkgs = ["dotnet-sdk_10"]
```

---

## Bonus — Switch to PostgreSQL with Npgsql

If you chose PostgreSQL in Step 4, make these two changes:

**Lesson39Api.csproj** — swap the package:

```xml
<PackageReference Include="Npgsql" Version="9.0.3" />
```

**Program.cs** — use the Npgsql connection class:

```csharp
using Npgsql;

// Replace SqlConnection with NpgsqlConnection
// Replace SqlCommand   with NpgsqlCommand
// Replace SqlException  with NpgsqlException (or just Exception)

// And change the INSERT to use PostgreSQL syntax:
var sql = @"INSERT INTO Students (FirstName, LastName, Email, Score)
            VALUES (@First, @Last, @Email, @Score)
            RETURNING Id;";
// ExecuteScalar() still works — it returns the new Id
```

PostgreSQL is free on Railway and has no Windows-only restrictions, which makes
it a common choice for .NET apps deployed to Linux servers.

---

## What you built

- A production C# Web API running on Railway
- No secrets in source code — all credentials in environment variables
- CORS configured so only your frontend can call the API
- A `/health` endpoint so Railway can verify the app is running
- Auto-deploy on every `git push`

In the next lesson you will connect the Vercel frontend to this live API URL.
