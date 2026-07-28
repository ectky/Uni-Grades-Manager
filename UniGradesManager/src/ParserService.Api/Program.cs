using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ParserService.Clients;
using ParserService.Services;

var builder = WebApplication.CreateBuilder(args);

// .NET Aspire: service defaults (health checks, OpenTelemetry, service discovery)
builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// See UserService.Api/Program.cs for why every browser-facing service needs
// its own CORS policy — there's no API Gateway to handle this centrally.
// Especially relevant here: the multipart file upload in POST /api/parse
// still has to clear a CORS preflight like any other cross-origin request.
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT bearer auth — every service validates the token independently (no API Gateway).
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
        };
    });
builder.Services.AddAuthorization();

// Named HttpClients resolved via Aspire service discovery.
// "https+http://courses-service" / "grades-service" match the names given to
// builder.AddProject<Projects.CourseService_Api>("courses-service") etc. in the AppHost.
builder.Services.AddHttpClient<ICoursesServiceClient, CoursesServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://courses-service");
});

builder.Services.AddHttpClient<IGradesServiceClient, GradesServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://grades-service");
});

builder.Services.AddScoped<IExcelGradeParserService, ExcelGradeParserService>();

var app = builder.Build();

app.MapDefaultEndpoints(); // Aspire: /health, /alive

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();