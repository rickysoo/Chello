# Lesson 33: Full Stack Project Setup

## What You Will Learn

In this lesson, you will set up a **full stack application** — a system where a frontend (what users see) talks to a backend API (which handles data), which talks to a database (which stores data).

This is how real-world web applications work.

---

## The Architecture

```
+------------------+       HTTP        +------------------+       SQL        +------------------+
|                  |  -------------->  |                  |  ------------->  |                  |
|  React Frontend  |                  |  C# ASP.NET API  |                  |  SQL Server DB   |
|  (Browser)       |  <--------------  |  (Backend)       |  <-------------  |  (ChelloApp)     |
|                  |    JSON data      |                  |    Query results |                  |
+------------------+                  +------------------+                  +------------------+
   localhost:3000                         localhost:5000                       localhost\SQLEXPRESS
```

**Three layers:**
1. **Frontend** — React app running in the browser (port 3000)
2. **Backend API** — C# ASP.NET Web API serving JSON (port 5000)
3. **Database** — SQL Server storing the actual data

---

## Folder Structure

```
Lesson33/
├── README.md               <- This file
├── api/                    <- C# ASP.NET Web API (backend)
│   ├── Lesson33Api.csproj
│   └── Program.cs
└── frontend/               <- React app (frontend)
    └── index.html
```

---

## Tech Stack

| Layer    | Technology              | Why                                      |
|----------|-------------------------|------------------------------------------|
| Frontend | React (via CDN)         | Simple, no build tools needed to start   |
| Backend  | C# ASP.NET Web API      | Same C# you have been learning           |
| Database | SQL Server Express      | Same database from Lesson 32             |
| Protocol | HTTP + JSON             | How the frontend and backend talk        |

---

## Prerequisites

- SQL Server Express installed (from Lesson 32)
- .NET 10.0 SDK installed
- The `ChelloApp` database with a `Students` table (from Lesson 32)
- A browser (Chrome, Edge, Firefox)

If you do not have the Students table yet, run this SQL in SSMS:

```sql
CREATE DATABASE ChelloApp;
GO

USE ChelloApp;
GO

CREATE TABLE Students (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    Age INT
);

INSERT INTO Students (Name, Email, Age) VALUES
('Alice Johnson', 'alice@example.com', 28),
('Bob Smith', 'bob@example.com', 34),
('Carol White', 'carol@example.com', 25);
```

---

## How to Run

### Step 1: Start the Backend API

Open a terminal, navigate to the `api` folder, and run:

```bash
cd Lesson33/api
dotnet run
```

You should see:
```
Now listening on: http://localhost:5000
```

You can test it by opening http://localhost:5000/api/students in your browser.

### Step 2: Open the Frontend

Open the `frontend/index.html` file directly in your browser.

You can do this by:
- Double-clicking the file in Windows Explorer, OR
- Dragging it into your browser window

The React app will load and fetch students from the API automatically.

---

## What is CORS?

**CORS** stands for Cross-Origin Resource Sharing.

When your React app (at `localhost:3000`) tries to call your API (at `localhost:5000`), the browser blocks it by default. This is a security feature — browsers do not allow one website to freely call another website's API without permission.

CORS is the mechanism that lets your API say: "I give permission for `localhost:3000` to call me."

Without CORS configured on the API, you will see this error in the browser console:
```
Access to fetch at 'http://localhost:5000/api/students' from origin 
'http://localhost:3000' has been blocked by CORS policy.
```

In the API's `Program.cs`, you will see exactly where CORS is configured and enabled.

---

## API Endpoints

| Method | URL                          | What it does              |
|--------|------------------------------|---------------------------|
| GET    | /api/students                | Get all students          |
| GET    | /api/students/{id}           | Get one student by ID     |
| POST   | /api/students                | Create a new student      |
| PUT    | /api/students/{id}           | Update an existing student|
| DELETE | /api/students/{id}           | Delete a student          |

---

## Key Concepts in This Lesson

- **Full stack** — combining frontend + backend + database
- **REST API** — a standard way to design backend endpoints
- **CORS** — how browsers control cross-origin requests
- **JSON** — the data format used between frontend and backend
- **Fetch API** — JavaScript's built-in tool for making HTTP requests
