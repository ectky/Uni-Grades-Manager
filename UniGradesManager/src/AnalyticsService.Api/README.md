# Analytics Service

No database of its own (Chapter 2.3). Every endpoint is a live aggregation:
call Grades Service for raw values, do the math, optionally enrich with
names from User Service or course info from Courses Service, return.

## Endpoints

| Method | Endpoint                          | Role                |
|--------|-----------------------------------|----------------------|
| GET    | /api/analytics/course/{id}         | All                  |
| GET    | /api/analytics/course/{id}/distribution | All             |
| GET    | /api/analytics/course/{id}/grades   | Instructor, Admin    |
| GET    | /api/analytics/student/{id}         | Student, Instructor  |
| GET    | /api/analytics/student/{id}/grades  | Student, Instructor  |

This is also **the only place students see their own courses at all** — see
`CourseService.Api/README.md` for why `GET /api/courses/my` dropped the
`Student` role: without an Enrollments table, only the presence of a Grade
row ties a student to a course, and only Analytics Service (via Grades
Service) can see that.

## Statistics — `GradeStatisticsCalculator`

Pure math, no I/O, kept in its own static class specifically so it's cheap
to unit test in isolation from the HTTP clients. A few choices worth
flagging:

- **Pass rate threshold is 3.00** (`PassingThreshold`), matching the
  Bulgarian scale where 2.00 is "Слаб" (fail) and 3.00 is the lowest passing
  grade ("Среден"). Change the constant if that's wrong for your institution.
- **Mode is computed on grades rounded to whole numbers**, not raw decimal
  values. With continuous grades like `5.50`, an exact-value mode is rarely
  meaningful, and rounding matches how the distribution chart already
  buckets grades in Fig. 3.3/3.7 — the two numbers you see on screen are
  consistent with each other because of this.
- **Standard deviation is population, not sample** (`/ n`, not `/ (n-1)`) —
  the values Analytics Service sees for a course *are* the entire gradebook
  for that offering, not a sample drawn from a larger population.
- **Distribution buckets by rounded grade, 2 through 6** — matches every
  earlier chart mockup (Fig. 3.3, 3.7), which always showed exactly five bars.

## Enrichment — two different patterns, on purpose

- **`GetCourseGradesAsync` batches.** One call to
  `IUsersServiceClient.GetNamesByIdsAsync` with every StudentId in the
  course, not one call per student — this is exactly why User Service's
  `GET /api/users/batch` exists in the first place.
- **`GetStudentGradesAsync` doesn't batch — it loops.** Courses Service has
  no equivalent "batch by ids" endpoint (only User Service does), so this
  calls `GetCourseAsync` once per distinct course the student has a grade
  in. Fine at "courses one student takes" scale (a handful per semester);
  if you ever need this at a scale where that stops being true, add a batch
  endpoint to Courses Service mirroring User Service's, rather than
  parallelizing N sequential internal calls.

## Authorization — two different shapes, on purpose

- **Course-level endpoints don't check ownership.** Any authenticated user
  can call `/api/analytics/course/{id}` or `.../distribution` — the data
  isn't sensitive at the aggregate level. `.../grades` (the per-student
  breakdown) is Instructor/Admin only via `[Authorize(Roles=...)]`, with no
  further check that the Instructor actually teaches that course — Приложение
  №1 documents the role as plain "Instructor, Admin" with no per-course
  ownership rule, so none is enforced here. Add one if you decide instructors
  should only see their own courses' student-level detail.
- **Student-level endpoints do check ownership**, in
  `AnalyticsService.EnsureCanViewStudent`: a Student can only view their own
  aggregate/grades (`requestingUserId == studentId`), an Instructor can view
  any student's (same reasoning as above — no per-course roster exists to
  narrow that further), Admin is irrelevant here since Приложение №1 never
  gave Admin this endpoint at all.

## AppHost wiring

Already added to `GradingSystem.AppHost/Program.cs` — see the (no longer
commented-out) block referencing `grades-service`, `courses-service`, and
`user-service`, plus the same `Jwt__*` and `Internal__ApiKey` environment
variables every other JWT-validating / internally-calling service gets.

## Files

```
AnalyticsService.Api/
├── Controllers/AnalyticsController.cs
├── Services/
│   ├── IAnalyticsService.cs / AnalyticsService.cs
│   └── GradeStatisticsCalculator.cs
├── Clients/
│   ├── IGradesServiceClient.cs / GradesServiceClient.cs
│   ├── IUsersServiceClient.cs + UsersServiceClient.cs (same file)
│   └── ICoursesServiceClient.cs + CoursesServiceClient.cs (same file)
├── Dtos/AnalyticsDtos.cs, UpstreamDtos.cs
├── Exceptions/AnalyticsExceptions.cs
├── Extensions/ClaimsPrincipalExtensions.cs
├── Program.cs
└── AnalyticsService.Api.csproj
```
