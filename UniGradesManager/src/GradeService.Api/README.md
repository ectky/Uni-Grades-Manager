# Grades Service

Stores grades. Nothing else — no update/finalize workflow, no student/course
enrichment, no public reads. Per the final architecture decision in this
project: **all readable, human-facing grade data goes through Analytics
Service**, which aggregates and enriches this service's raw rows with names
and course info from User Service / Courses Service.

## Endpoints — all internal, none of them public API

| Method | Endpoint                 | Caller             |
|--------|---------------------------|---------------------|
| POST   | /api/grades                | Parser Service only |
| GET    | /api/grades/course/{id}    | Analytics Service only |
| GET    | /api/grades/student/{id}   | Analytics Service only |

These do **not** appear in Приложение №1 (the public API documentation) —
they're plumbing between services, not something the frontend calls. That's
also why they're not protected with `[Authorize(Roles = "...")]`: there's no
end-user JWT role that makes sense here. Instead, `InternalOnlyAttribute`
checks a shared secret header (`X-Internal-Api-Key`) that Parser Service,
Analytics Service, and Grades Service all receive from the same Aspire
configuration (`Internal:ApiKey`). Since the system has no central API
Gateway to enforce this boundary in one place, each service enforces it on
itself.

**If you want something stronger than a shared static key** (e.g. mutual TLS
between services, or forwarding the caller's own JWT and checking for an
internal-service claim), swap out `InternalOnlyAttribute` — the controller
and service layer don't care how the boundary is enforced.

## Upsert semantics

Re-uploading an Excel file for the same course does not create duplicates.
`GradeRepository.UpsertManyAsync` loads existing `(CourseId, StudentId)` rows
for the incoming batch in one query, updates `Value` on matches, inserts the
rest, and commits both in a single transaction. This matches Chapter 3.6:
*"повторно качване на файл за същия курс презаписва съответните стойности"*.
The unique index on `(CourseId, StudentId)` in `GradesDbContext` enforces
this invariant at the database level too, not just in application code.

## Validation (defense in depth)

Parser Service already validates the Excel file before calling here, but
`GradeService.UploadGradesAsync` re-checks independently, since Grades
Service should not blindly trust any caller, internal or not:

- `CourseId` must be positive.
- The batch must be non-empty.
- No duplicate `StudentId` within one batch (ambiguous — which value would
  "win"?).
- Each `Value` must fall inside `[2.00, 6.00]` (Bulgarian grading scale —
  change `MinGradeValue`/`MaxGradeValue` if that's wrong for your institution).

A failed check throws `InvalidGradeBatchException`, mapped to `400 Bad
Request` in the controller — the whole batch is rejected, not partially
applied.

## Open issue carried over from the documentation review

`Grade.Value` is a C# `double`. Приложение №2 at one point described the
`Grades` table's `Value` column as `decimal(4,2)`. Those are different SQL
column types (`float(53)` vs `decimal`), and forcing one onto the other in
`OnModelCreating` **without a value converter** would silently corrupt data.
I left `Value` mapped as `double` → `float(53)` here, matching the entity
you gave me literally. If you want true `decimal` precision:

1. Change `Grade.Value` to `decimal`.
2. Update `GradeRecordDto`/`GradeDto`/`BatchGradeUploadDto` in **both** this
   service and Parser Service to `decimal` as well (a `double` on one side
   and `decimal` on the other will fail to (de)serialize consistently).
3. Only then map the SQL column type explicitly.

## AppHost wiring this expects

```csharp
var gradesDb = builder.AddSqlServer("sql")
    .AddDatabase("grades-db");

builder.AddProject<Projects.GradeService_Api>("grades-service")
    .WithReference(gradesDb);
```

Plus a shared `Internal:ApiKey` value injected into Parser Service, Analytics
Service, and Grades Service — e.g. via `builder.AddParameter("internal-api-key", secret: true)`
in the AppHost, passed to all three with `.WithEnvironment("Internal__ApiKey", internalApiKey)`.

## Files

```
GradeService/
├── Controllers/GradesController.cs
├── Services/IGradeService.cs, GradeService.cs
├── Repositories/IGradeRepository.cs, GradeRepository.cs
├── Domain/Entities/Grade.cs
├── Dtos/GradeDtos.cs
├── Exceptions/InvalidGradeBatchException.cs
├── Security/InternalOnlyAttribute.cs
├── Data/GradesDbContext.cs
├── Program.cs
└── GradeService.csproj
```
