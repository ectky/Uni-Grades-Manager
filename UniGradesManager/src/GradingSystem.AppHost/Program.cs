using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ---- Shared secrets, injected identically into every service that needs them ----
var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);
var internalApiKey = builder.AddParameter("internal-api-key", secret: true);

const string jwtIssuer = "grading-system";
const string jwtAudience = "grading-system-clients";

// Optional — only used by UserService.Api/Data/DevelopmentSeeder.cs to create
// the very first Admin account on an empty database (Development only).
// Reads from the AppHost's own configuration if you set one (e.g. via
// `dotnet user-secrets set SeedAdminPassword "..."` in this project), else
// falls back to the same literal defaults the seeder itself falls back to.
var seedAdminEmail = builder.Configuration["SeedAdminEmail"] ?? "admin@edugrade.local";
var seedAdminPassword = builder.Configuration["SeedAdminPassword"] ?? "ChangeMe123!";

// The Vite dev server's default port. Override via
// `dotnet user-secrets set FrontendOrigin "http://localhost:XXXX"` in this
// project if you run the frontend on a different port.
var frontendOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173";

// ---- Database per Service (Chapter 2.6) ----
// Using AddConnectionString instead of AddSqlServer: Aspire does NOT manage
// or start anything here — no Docker container. Each line just declares a
// named reference; the actual connection string comes from configuration
// (see the AppHost's own appsettings.json / user-secrets), pointing at
// whatever SQL Server instance you already have installed locally.
var usersDb = builder.AddConnectionString("users-db");
var gradesDb = builder.AddConnectionString("grades-db");
var coursesDb = builder.AddConnectionString("courses-db");

// ---- User Service ----
// Issues + validates JWTs; also validates them for its own /api/users/{id} and
// /api/auth/register. Exposed externally — the frontend calls it directly
// (no API Gateway, per Chapter 2.3).
var userService = builder.AddProject<Projects.UserService_Api>("user-service")
    .WithReference(usersDb)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Internal__ApiKey", internalApiKey)
    .WithEnvironment("Seed__AdminEmail", seedAdminEmail)
    .WithEnvironment("Seed__AdminPassword", seedAdminPassword)
    .WithEnvironment("Cors__FrontendOrigin", frontendOrigin)
    .WithHttpsEndpoint(port: 7001)
    .WithExternalHttpEndpoints();

// ---- Grades Service ----
// Deliberately NOT given .WithExternalHttpEndpoints() — it has no public
// endpoints at all (Приложение №1). Only reachable via Aspire service
// discovery, by Parser Service (writes) and Analytics Service (reads).
// Not pinned to a fixed port either, for the same reason: nothing outside
// this AppHost ever needs to type its address by hand.
var gradeService = builder.AddProject<Projects.GradeService_Api>("grades-service")
    .WithReference(gradesDb)
    .WithEnvironment("Internal__ApiKey", internalApiKey);

// ---- Courses Service ----
// Validates JWTs for its user-facing endpoints (GET /api/courses, /my, POST,
// PUT), and separately guards GET /api/courses/{id} with the internal key
// for Parser Service / Analytics Service. Exposed externally for the frontend.
var courseService = builder.AddProject<Projects.CourseService_Api>("courses-service")
    .WithReference(coursesDb)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Internal__ApiKey", internalApiKey)
    .WithEnvironment("Cors__FrontendOrigin", frontendOrigin)
    .WithHttpsEndpoint(port: 7002)
    .WithExternalHttpEndpoints();

// ---- Parser Service ----
// Validates the Instructor's JWT for its own endpoints, then calls Courses
// Service (existence check) and Grades Service (write) internally using the
// shared Internal__ApiKey. Exposed externally — the frontend uploads
// directly to it.
builder.AddProject<Projects.ParserService_Api>("parser-service")
    .WithReference(courseService)
    .WithReference(gradeService)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Internal__ApiKey", internalApiKey)
    .WithEnvironment("Cors__FrontendOrigin", frontendOrigin)
    .WithHttpsEndpoint(port: 7003)
    .WithExternalHttpEndpoints();

// ---- Analytics Service ----
// No database of its own — reads Grades Service, enriches with names/course
// info from User Service and Courses Service. Exposed externally, since the
// frontend calls it directly for both dashboards and both stats pages.
builder.AddProject<Projects.AnalyticsService_Api>("analytics-service")
    .WithReference(gradeService)
    .WithReference(courseService)
    .WithReference(userService)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Internal__ApiKey", internalApiKey)
    .WithEnvironment("Cors__FrontendOrigin", frontendOrigin)
    .WithHttpsEndpoint(port: 7004)
    .WithExternalHttpEndpoints();

builder.Build().Run();