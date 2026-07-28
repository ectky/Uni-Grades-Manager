# EduGrade — Frontend

React + Vite SPA implementing every screen from Chapter 3 (Фиг. 3.1–3.7),
wired to the final API contracts from Приложение №1. No API Gateway — the
app calls each microservice directly (see `.env.example`), and Grades
Service has no client here at all, since it has no public endpoints;
everything grade-related goes through Analytics Service.

## Setup

```bash
npm install
cp .env.example .env   # fill in the real URLs Aspire assigns each service
npm run dev
```

## Structure

```
src/
├── api/            one file per microservice (authApi, coursesApi, parseApi, analyticsApi)
│                   + httpClient.js (axios instances, JWT header, 401 handling)
├── auth/            AuthContext, ProtectedRoute, jwt.js (decode-only, no verify)
├── components/      Sidebar, AppLayout, GradeDistributionChart, Common (StatCard/Loading/Error)
└── pages/
    ├── LoginPage.jsx                          Fig. 3.1
    ├── student/StudentDashboardPage.jsx        Fig. 3.2
    ├── student/PublicCourseStatsPage.jsx       Fig. 3.3 (shared with Instructor)
    ├── instructor/InstructorDashboardPage.jsx  Fig. 3.4
    ├── instructor/UploadGradesPage.jsx         Fig. 3.5 + 3.6 (one page, two states)
    ├── instructor/DetailedCourseStatsPage.jsx  Fig. 3.7
    ├── admin/AdminUsersPage.jsx                 not in the original mockups — see below
    └── admin/AdminCoursesPage.jsx               not in the original mockups — see below
```

## Things I had to decide that you should check

**1. Analytics Service response shapes — now confirmed, not assumed.**
`AnalyticsService.Api` exists in the solution now (`src/AnalyticsService.Api`)
and its DTOs (`CourseStatsDto`, `DistributionItemDto`, `CourseGradeDto`,
`StudentStatsDto`, `StudentGradeDto`) match exactly what
`src/api/analyticsApi.js` already expected — same field names, same shapes.
Nothing to reconcile.

**2. Two admin screens exist that were never in Chapter 3's mockups.**
`AdminUsersPage` (calls `POST /api/auth/register`) and `AdminCoursesPage`
(calls `POST /api/courses`) had to exist somewhere — without them, no
Instructor/Student account or course could ever be created after the first
Admin. Both say so in a code comment. Restyle them however fits the rest of
your UI; the important part is that they call the right endpoints.

**3. `.env` needs real per-service URLs, not Aspire's service-discovery names.**
`https+http://user-service` (used inside the .NET services' own HttpClients)
only resolves for server-to-server calls — a browser can't use it. Check the
Aspire Dashboard for the actual `https://localhost:PORT` each project gets,
or fix them with `.WithExternalHttpEndpoints()` in the AppHost, and put those
real URLs in `.env`.

**4. JWT claim reading assumes ASP.NET Core's default long-form claim types.**
`TokenService` (User Service) writes claims using `ClaimTypes.NameIdentifier`
/ `ClaimTypes.Role` / etc., which serialize into the JWT as their full URI
strings (e.g. `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role`),
not short names like `sub`/`role`. `src/auth/jwt.js` checks both forms — if
you change how User Service issues claims, check that decoder still matches.

**5. Grade values are treated as plain numbers (JS `number`), matching the
`double` in `Grade.cs`.** If you switch `Grade.Value` to `decimal` later (an
open item from the Grades Service review), nothing changes here — JSON
doesn't distinguish the two, only the backend's (de)serialization does.

## What's intentionally not here

- No grade editing/finalization UI — matches the decision that Grades
  Service has no update endpoint at all; the only way a grade changes is by
  re-uploading an Excel file (Chapter 3.6).
- No "pending grade" rows on the student dashboard — removed earlier, since
  without an Enrollments table there's no data source for "courses I'm
  waiting on a grade for," only courses that already have one.
- No password reset / forgot-password screen — explicitly declined earlier
  ("не е нужна за целите на дипломната").
