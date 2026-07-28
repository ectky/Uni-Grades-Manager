using System.Text;
using AnalyticsService.Clients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// See UserService.Api/Program.cs for why every browser-facing service needs
// its own CORS policy — there's no API Gateway to handle this centrally.
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

// Analytics Service has no database of its own — everything below is a
// service-to-service call, resolved via .NET Aspire service discovery and
// authenticated with the shared Internal:ApiKey (see each client).
builder.Services.AddHttpClient<IGradesServiceClient, GradesServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://grades-service");
});
builder.Services.AddHttpClient<IUsersServiceClient, UsersServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://user-service");
});
builder.Services.AddHttpClient<ICoursesServiceClient, CoursesServiceClient>(client =>
{
    client.BaseAddress = new Uri("https+http://courses-service");
});

builder.Services.AddScoped<AnalyticsService.Services.IAnalyticsService, AnalyticsService.Services.AnalyticsService>();

var app = builder.Build();

app.MapDefaultEndpoints();

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