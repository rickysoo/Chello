# Chello — Full-Stack Web Dev Course

A 40-lesson hands-on course that takes you from C# basics all the way to a fully deployed full-stack web application. Each lesson builds on the last, and every phase ends with something you built yourself.

## Tech Stack

```
React (Frontend)
HTML + CSS + JavaScript
        |
        | HTTP / JSON
        v
C# ASP.NET Web API (Backend)
        |
        | SQL queries
        v
MS SQL Server (Database)
```

---

## Course Phases

### Phase 1: C# Basics (Lessons 1–8) ✅
*Learn the language before connecting it to anything*

| # | Lesson | What You Build |
|---|--------|----------------|
| 01 | Hello World & Variables | Print text, store values |
| 02 | If/Else Decisions | Number guessing game |
| 03 | Loops | Upgraded guessing game with retries |
| 04 | Methods | Simple calculator |
| 05 | Lists & Arrays | To-do list app |
| 06 | Classes & Objects | Bank account with deposits & withdrawals |
| 07 | Error Handling | Crash-proof calculator with try/catch |
| 08 | File Read & Write | To-do list that saves to disk |

---

### Phase 2: MS SQL + C# (Lessons 9–14) ✅
*Connect your app to a real database*

| # | Lesson | What You Build |
|---|--------|----------------|
| 09 | SQL Basics | Create database, table, insert, select, update, delete |
| 10 | C# Connects to SQL | Read data from DB into your C# app |
| 11 | CRUD Operations | Create, Read, Update, Delete records from C# |
| 12 | SQL Queries | Filter, sort, search, and aggregate data |
| 13 | Stored Procedures | Run DB logic from C# using named procedures |
| 14 | Mini Project | Student grade tracker — full CRUD console app |

---

### Phase 3: C# Web API (Lessons 15–20) 🔜
*Build the backend that a browser can talk to*

| # | Lesson | What You Build |
|---|--------|----------------|
| 15 | What is an API? | First ASP.NET Web API project |
| 16 | GET Endpoints | Return data as JSON |
| 17 | POST Endpoints | Receive and save new data |
| 18 | PUT & DELETE | Update and remove records |
| 19 | Connect API to SQL | Full CRUD API backed by SQL Server |
| 20 | API Security | Add basic authentication |

---

### Phase 4: HTML & CSS (Lessons 21–25) 🔜
*Build web pages before adding React*

| # | Lesson | What You Build |
|---|--------|----------------|
| 21 | HTML Basics | Your first webpage |
| 22 | Forms & Inputs | Login form, contact form |
| 23 | CSS Styling | Make it look good |
| 24 | Responsive Design | Works on mobile too |
| 25 | Fetch API | Call your C# API from a web page |

---

### Phase 5: React Frontend (Lessons 26–32) 🔜
*Build a modern interactive UI*

| # | Lesson | What You Build |
|---|--------|----------------|
| 26 | React Basics | First React app, components |
| 27 | Props & State | Dynamic data on screen |
| 28 | Forms in React | Controlled inputs, form submission |
| 29 | Fetch from API | Connect React to your C# API |
| 30 | React Router | Multiple pages in one app |
| 31 | Loading & Errors | Handle slow and failed API calls |
| 32 | Styling React | Clean UI with CSS modules |

---

### Phase 6: Full Stack Project (Lessons 33–40) 🔜
*Build one complete app end to end*

| # | Lesson | What You Build |
|---|--------|----------------|
| 33 | Project Setup | React + C# API + SQL wired together |
| 34 | User Registration | Sign up, save to DB |
| 35 | User Login | Authenticate with JWT |
| 36 | Dashboard | Show user data from DB |
| 37 | Full CRUD UI | Add, edit, delete items |
| 38 | Search & Filter | Live search with DB queries |
| 39 | Deploy Backend | Publish C# API to a server |
| 40 | Deploy Frontend | Host React app online |

---

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 8.0 or higher)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (for Lessons 9–14+)
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (recommended)
- A code editor — [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with the C# extension

### Running a C# Lesson

```bash
cd Lesson01
dotnet run
```

### Running a SQL Lesson (Lesson 9, 13)

Open the `.sql` files in SSMS and execute them in order (01 → 02 → ...).

---

## Tips

- Work through lessons **sequentially** — each one builds on the last
- **Read the comments** in every file — they explain the why, not just the what
- **Experiment** — change the code, break things, fix them
- Don't rush Phase 1 — strong fundamentals make every later phase easier

---

**40 lessons. By Lesson 40 you'll have a fully deployed full-stack web app.**
