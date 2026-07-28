# Parser Service — Excel Grade Import

Implements the two endpoints from Приложение №1 / Fig. 3.5–3.6:

| Method | Endpoint            | Role       |
|--------|---------------------|------------|
| POST   | /api/parse          | Instructor |
| GET    | /api/parse/template | Instructor |

## Flow (matches Chapter 2.8)

1. Instructor selects an existing course (from `GET /api/courses/my` on the frontend) and uploads an `.xlsx` file to `POST /api/parse`.
2. `ExcelGradeParserService` validates the file (size ≤ 10 MB, `.xlsx` extension, required `StudentId`/`Value` columns, valid data rows).
3. It asks **Courses Service** whether `CourseId` exists (`CoursesServiceClient` → `GET /api/courses/{id}`). Parser Service never creates or edits courses — only Admin does that.
4. It writes the parsed grades to **Grades Service** in one batched call (`GradesServiceClient` → `POST /api/grades`), since Grades Service accepts writes only from Parser Service.
5. Returns a `ParseResultDto` — course name/period, record count, processing time, and the parsed rows — for the confirmation screen (Fig. 3.6).

## Design notes / decisions worth flagging back to you

- **Batched write, not one-row-at-a-time.** Приложение №1 documents `POST /api/grades` as a single endpoint without specifying batch vs. single-record. Calling it once per student for a 100+ row spreadsheet would be slow and non-atomic, so this implementation sends the whole parsed set in one `BatchGradeUploadDto { CourseId, List<GradeRecordDto> } ` call. If Grades Service's actual endpoint only accepts one record at a time, either change its contract to accept a batch, or change `GradesServiceClient` to loop (with retry/rollback handling, since partial failure becomes possible).
- **All-or-nothing file validation.** If any row is malformed, the whole file is rejected with a combined error message (`InvalidExcelStructureException`) rather than importing the valid rows and skipping bad ones. This matches "invalid or corrupted files" handling described in Chapter 4.4 — silently dropping rows on a gradebook felt riskier than rejecting the file outright. Tell me if partial import is actually what you want.
- **Grade range validated at parse time** (2.00–6.00, Bulgarian scale) — adjust `MinGradeValue`/`MaxGradeValue` if your institution uses a different scale.
- **No `.xls` (legacy binary) support** — only `.xlsx`, since ClosedXML doesn't read the old binary format. Fig. 3.5's copy already says ".xlsx, .xls", so either drop `.xls` from that screen's copy, or add a conversion step (e.g. via `NPOI`) before parsing.
- **Service discovery URIs** (`https+http://courses-service`, `https+http://grades-service` in `Program.cs`) assume the AppHost registers those projects under exactly those names — match them to whatever you finalize in Приложение №3.
- **JWT validation config** (`Jwt:Issuer`, `Jwt:Audience`, `Jwt:SigningKey`) is expected to come from `.NET Aspire`'s shared configuration, consistent with Chapter 4.2.

## Files

```
ParserService/
├── Controllers/ParseController.cs
├── Services/
│   ├── IExcelGradeParserService.cs
│   └── ExcelGradeParserService.cs
├── Clients/
│   ├── ICoursesServiceClient.cs / CoursesServiceClient.cs
│   └── IGradesServiceClient.cs / GradesServiceClient.cs
├── Dtos/ParseDtos.cs
├── Exceptions/ParserExceptions.cs
├── Program.cs
└── ParserService.csproj
```
