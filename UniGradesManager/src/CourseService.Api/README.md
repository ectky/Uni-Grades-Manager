# Courses Service

Owns course metadata only — name, period, owning instructor. No enrollment,
no grade-submission deadlines (removed earlier — see the comment in
`Course.cs`), no statistics. All of that lives elsewhere: Grades Service
stores the actual grades, Analytics Service aggregates and enriches.

## Endpoints

| Method | Endpoint             | Role                          |
|--------|----------------------|--------------------------------|
| GET    | /api/courses          | Admin                          |
| GET    | /api/courses/my       | Instructor                     |
| GET    | /api/courses/{id}     | Internal only (Parser, Analytics) |
| POST   | /api/courses          | Admin                          |
| PUT    | /api/courses/{id}     | Admin, or the owning Instructor |

## A role I removed from Приложение №1, and why

The API documentation at one point listed `GET /api/courses/my` as available
to **Student** as well as Instructor. That doesn't actually work: without an
Enrollments table (removed earlier in this project), Courses Service has no
way to know which courses belong to a given student — only Grades Service
does, implicitly, through the presence of a `(CourseId, StudentId)` row.
Analytics Service already exposes exactly that, enriched with course
name/period, via `GET /api/analytics/student/{id}/grades`. So a student's
"my courses" view is served from there, not from Courses Service directly.

I dropped `Student` from this endpoint's authorization policy accordingly.
**If you actually want students hitting this endpoint directly**, the honest
fix is bringing back an Enrollments concept — tell me and I'll add it back
along with the corresponding entity/migration, rather than silently
special-casing it here.

## Ownership check on PUT /api/courses/{id}

`CourseService.UpdateAsync` takes the caller's user id and role explicitly
(read from the JWT in the controller via `User.GetUserId()` /
`User.IsInRole("Admin")`) and only lets the request through if the caller is
Admin **or** `course.InstructorId` matches the caller. Anyone else gets a
`403 Forbidden` — not a 404, so an Instructor probing course ids they don't
own gets an honest "not allowed" rather than being told the course doesn't
exist (which would leak less, but 403 matches how the rest of this project
already handles ownership, e.g. Grades Service's batch validation).

## GET /api/courses/{id} — internal, not JWT-protected

This is the same pattern as Grades Service's internal endpoints: guarded by
`InternalOnlyAttribute` checking `X-Internal-Api-Key`, not by a user role,
since its only two callers are:

- **Parser Service**, confirming a `CourseId` exists before writing grades
  (`ICoursesServiceClient.GetCourseAsync` in Parser Service — **update that
  client to send the `X-Internal-Api-Key` header**, the same way
  `GradesServiceClient` already does; it currently doesn't, since it was
  written before Grades Service's internal-only convention existed).
- **Analytics Service**, enriching course-level statistics with a readable
  name/period.

## AppHost wiring this expects

```csharp
var coursesDb = builder.AddSqlServer("sql")
    .AddDatabase("courses-db");

builder.AddProject<Projects.CourseService_Api>("courses-service")
    .WithReference(coursesDb)
    .WithEnvironment("Internal__ApiKey", internalApiKey);
```

Same shared `Internal:ApiKey` parameter as Grades Service.

## Files

```
CourseService/
├── Controllers/CoursesController.cs
├── Services/ICourseService.cs, CourseService.cs
├── Repositories/ICourseRepository.cs, CourseRepository.cs
├── Domain/Entities/Course.cs
├── Dtos/CourseDtos.cs
├── Exceptions/CourseExceptions.cs
├── Security/InternalOnlyAttribute.cs
├── Extensions/ClaimsPrincipalExtensions.cs
├── Data/CoursesDbContext.cs
├── Program.cs
└── CourseService.csproj
```
