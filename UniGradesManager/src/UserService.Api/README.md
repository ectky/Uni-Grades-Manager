# User Service

Authentication, JWT issuance, and profile lookup. Per Приложение №1:

| Method | Endpoint            | Role        |
|--------|----------------------|-------------|
| POST   | /api/auth/login       | Public      |
| POST   | /api/auth/register    | Admin       |
| GET    | /api/users/{id}       | Admin, Owner |
| GET    | /api/users/batch      | Internal (Analytics Service only) |

No self-service sign-up, no profile self-edit, no Admin user-listing/
deactivation — all removed earlier in this project's Q&A round.

## Two fixes to the `User` entity you gave me — please review

I kept your PBKDF2/HMACSHA256 hashing logic completely unchanged. Two things
around it had to change for the class to actually work as a real EF Core
entity used by a login flow:

**1. `VerifyPassword` was `private`.** As given, nothing outside the `User`
class — including `AuthService`, which has to check a login attempt — could
ever call it. Made it `public`. The algorithm inside is untouched.

**2. The original constructor both took `id` and always hashed `password`.**
That's fine for *creating* a new user, but EF Core also needs a constructor
to *reconstruct* existing rows when reading from the database — and if it
had reused that same constructor, every login would re-hash an
already-hashed password read from the `Password` column, corrupting it
(and locking out every existing user the moment EF touched their row).

Fix: added a `private` parameterless constructor for EF Core's own use
(materialization only, no logic runs), and narrowed the public constructor
to `(name, email, password, type, studentId)` — no `id` parameter, since
that's an auto-generated identity column the caller can't meaningfully
supply before insert anyway. Properties are now `private set` so external
code can't mutate `Password` or `Type` directly, only through the
constructor or (if you add one later) an explicit method.

**If you'd rather keep the exact original 6-parameter constructor signature**
(with `id`), that's fine too — just also add the private parameterless one
alongside it; EF Core prefers a parameterless constructor when one exists,
so the two can coexist without EF ever routing through the hashing one.

## `StudentId` — still unresolved, carried through as-is

The comment *"Nullable StudentId for users of type Instructor"* is your
original wording. What this field is supposed to mean for a Student or Admin
user — and how a Student's actual student number relates to it — was never
settled in the discussion. I didn't invent an interpretation; the field
exists on the entity and in `RegisterUserDto`, but nothing in `AuthService`
or the controllers currently depends on its value. Don't build logic against
it until you've decided what it means.

## Login error handling

`LoginAsync` throws the same `InvalidCredentialsException` whether the email
doesn't exist or the password is wrong, and `AuthController` returns the same
generic 401 either way — this matches Chapter 4.5's "don't reveal which part
was wrong" requirement.

## GET /api/users/{id} — Admin or the profile's owner

Enforced in `UserProfileService.GetProfileAsync`, not just in the
`[Authorize]` attribute: any authenticated user can call the endpoint, but
the service throws `ForbiddenProfileAccessException` (→ `403`) unless the
caller is Admin or `id` matches their own JWT subject.

## GET /api/users/batch — internal only, minimal fields

Guarded by the same `InternalOnlyAttribute` / `X-Internal-Api-Key` pattern
used in Grades Service and Courses Service. Deliberately returns only `Id`
and `Name` (`UserBatchItemDto`) — Analytics Service needs a label to put next
to a StudentId, nothing more; there's no reason for an internal enrichment
call to also expose `Email`.

## AppHost wiring this expects

```csharp
var usersDb = builder.AddSqlServer("sql")
    .AddDatabase("users-db");

var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);
var internalApiKey = builder.AddParameter("internal-api-key", secret: true);

builder.AddProject<Projects.UserService_Api>("user-service")
    .WithReference(usersDb)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Jwt__Issuer", "grading-system")
    .WithEnvironment("Jwt__Audience", "grading-system-clients")
    .WithEnvironment("Internal__ApiKey", internalApiKey);
```

The same `Jwt__SigningKey` / `Jwt__Issuer` / `Jwt__Audience` values must be
injected identically into every other service that validates tokens
(Courses Service, and any future service that adds `[Authorize]`), and the
same `Internal__ApiKey` into Parser Service and Analytics Service.

## Files

```
UserService/
├── Controllers/AuthController.cs, UsersController.cs
├── Services/
│   ├── IAuthService.cs / AuthService.cs
│   ├── IUserProfileService.cs / UserProfileService.cs (same file)
│   └── ITokenService.cs / TokenService.cs (same file)
├── Repositories/IUserRepository.cs, UserRepository.cs
├── Domain/Entities/User.cs
├── Domain/Enums/UserType.cs
├── Dtos/AuthDtos.cs, UserDtos.cs
├── Exceptions/UserExceptions.cs
├── Security/InternalOnlyAttribute.cs
├── Extensions/ClaimsPrincipalExtensions.cs
├── Data/UsersDbContext.cs
├── Program.cs
└── UserService.csproj
```
