# Lesson 40: Deploy Frontend — Host React App on Vercel

## What You Will Learn

In this final lesson, you will deploy your React frontend to Vercel so the world can access your full-stack Student Management app. You will also connect your frontend to the live backend API you deployed earlier.

---

## Step 1: Create a Vercel Account

1. Go to [https://vercel.com](https://vercel.com)
2. Click **Sign Up**
3. Choose **Continue with GitHub** (recommended — makes deployment automatic)
4. Authorize Vercel to access your GitHub account

---

## Step 2: Push Your Frontend to GitHub

Your frontend files must be in a GitHub repository before Vercel can deploy them.

```bash
# If you do not have a repo yet, create one on github.com first, then:
git init
git add .
git commit -m "Lesson 40 - Final full-stack React frontend"
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
git push -u origin main
```

---

## Step 3: Import Your Project on Vercel

1. Log in to [https://vercel.com/dashboard](https://vercel.com/dashboard)
2. Click **Add New > Project**
3. Find your GitHub repository in the list and click **Import**
4. Vercel will auto-detect your project settings

**Settings to configure:**
- **Framework Preset:** Other (since this is a plain HTML site)
- **Root Directory:** `Lesson40/frontend` (or wherever your `index.html` lives)
- **Build Command:** leave empty
- **Output Directory:** leave empty (or set to `.`)

5. Click **Deploy**

Vercel will give you a live URL like `https://your-project.vercel.app` within about 30 seconds.

---

## Step 4: Set Environment Variables in Vercel

Your frontend needs to know the URL of your backend API. Rather than hardcoding it, you configure it in Vercel so it is easy to change.

**In the Vercel Dashboard:**
1. Go to your project
2. Click **Settings > Environment Variables**
3. Add a new variable:
   - **Name:** `VITE_API_URL` (or note: since this is a plain HTML file, you will update the `API_BASE_URL` constant directly in `index.html` before deploying)
   - **Value:** `https://your-backend.railway.app` (your deployed backend URL from Lesson 39)
4. Click **Save**
5. Redeploy for the change to take effect: go to **Deployments > Redeploy**

> **Note for plain HTML apps:** Because `index.html` is a static file loaded directly in the browser, it cannot read server-side environment variables at runtime. Instead, update the `API_BASE_URL` constant near the top of `index.html` to your production backend URL before pushing to GitHub.

```javascript
// In frontend/index.html — change this line before deploying:
const API_BASE_URL = "https://your-backend.railway.app";
```

---

## Step 5: Optional — Set Up a Custom Domain

If you own a domain name (e.g., from Namecheap or GoDaddy), you can point it to your Vercel app.

1. In Vercel, go to your project **Settings > Domains**
2. Click **Add Domain**
3. Type your domain name (e.g., `app.yourdomain.com`)
4. Vercel will show you DNS records to add (usually a `CNAME` record)
5. Log in to your domain registrar and add those DNS records
6. Wait a few minutes — Vercel automatically provisions an SSL certificate

Your app will then be accessible at your custom domain with HTTPS.

---

## How Frontend and Backend Work Together in Production

When a user visits your Vercel URL:

1. **Browser** requests `index.html` from Vercel's CDN (super fast, global)
2. **React app** loads in the browser
3. User logs in — the React app sends a `POST /api/auth/login` request to your **Railway backend**
4. Railway backend checks credentials against the **PostgreSQL database**
5. Backend returns a JWT token
6. React app stores the token and uses it for all future API calls
7. All student data (create, read, update, delete) flows between the React app and your Railway API

The frontend and backend are completely separate services — this is called a **decoupled architecture**. It means you can update your frontend without touching your backend, and vice versa.

---

## Final Architecture Diagram

```
                        PRODUCTION ARCHITECTURE
                        =======================

  User's Browser
  +------------------+
  |                  |
  |  React App       |  <-- Loaded from Vercel CDN
  |  (index.html)    |       https://your-app.vercel.app
  |                  |
  +--------+---------+
           |
           | HTTP requests (fetch API)
           | Authorization: Bearer <JWT token>
           |
           v
  +------------------+
  |                  |
  |  Backend API     |  <-- Hosted on Railway
  |  (ASP.NET Core)  |       https://your-backend.railway.app
  |                  |
  +--------+---------+
           |
           | Entity Framework Core
           | SQL queries
           |
           v
  +------------------+
  |                  |
  |  PostgreSQL DB   |  <-- Hosted on Railway (or Supabase)
  |                  |       Persistent data storage
  |                  |
  +------------------+


  FLOW:
  Browser --> Vercel (static files) --> loads React app
  React app --> Railway API --> PostgreSQL
                             <-- JSON response
  React app <-- displays data to user
```

---

## What You Have Built Across 40 Lessons

| Lessons | Topic |
|---------|-------|
| 01-05   | C# basics: variables, types, loops, conditions |
| 06-10   | Methods, arrays, lists, error handling, SQL basics |
| 11-15   | OOP: classes, objects, inheritance, interfaces |
| 16-20   | Advanced C#: LINQ, generics, async/await, delegates |
| 21-25   | ASP.NET Core: Web API, routing, controllers, models |
| 26-30   | Database: Entity Framework, migrations, CRUD, relationships |
| 31-35   | Authentication: JWT, login, protected endpoints, refresh tokens |
| 36-38   | React frontend: CDN setup, fetch API, state management |
| 39      | Deploy backend to Railway |
| 40      | Deploy frontend to Vercel |

---

## Congratulations — Course Complete!

You started with `Console.WriteLine("Hello World")` in Lesson 01.

You have now built and deployed a **complete, production-ready full-stack web application** with:

- A C# / ASP.NET Core REST API
- A PostgreSQL database with Entity Framework Core
- JWT-based authentication
- A React frontend with login, dashboard, and full CRUD
- Live backend deployed on Railway
- Live frontend deployed on Vercel

That is a real application running on the internet. You built it from scratch.

**You are now a full-stack developer.**

The skills you have learned in this course are the same skills used by professional developers at companies around the world. Everything from here is about building more things, going deeper, and learning by doing.

Keep building. Keep shipping.

---

## What to Learn Next

- **TypeScript** — add type safety to your React frontend
- **Next.js** — React framework with server-side rendering
- **Docker** — containerize your backend for easier deployment
- **CI/CD** — automate tests and deployments with GitHub Actions
- **Redis** — add caching to your API for better performance
- **Unit Testing** — write tests for your C# API with xUnit
