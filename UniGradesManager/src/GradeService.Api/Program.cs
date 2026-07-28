using GradeService.Data;
using GradeService.Repositories;
using GradeService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// .NET Aspire: service defaults (health checks, OpenTelemetry, service discovery)
builder.AddServiceDefaults();

// Aspire SQL Server integration — "grades-db" matches the database resource name
// declared in the AppHost (builder.AddSqlServer(...).AddDatabase("grades-db")),
// and injects the connection string automatically. Database per Service (Chapter 2.6).
builder.AddSqlServerDbContext<GradesDbContext>("grades-db");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IGradeService, GradeService.Services.GradeService>();

var app = builder.Build();

app.MapDefaultEndpoints(); // Aspire: /health, /alive

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Apply pending migrations automatically in local/dev runs under Aspire.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GradesDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
