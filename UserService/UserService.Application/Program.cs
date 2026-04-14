using EntityFrameworkCore.UseRowNumberForPaging;
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<UserServiceDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:UserServiceDatabase"], r => { r.UseRowNumberForPaging();
        r.EnableRetryOnFailure();
    });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    // Automatically update database
    var retryCount = 5;
    for (int i = 0; i < retryCount; i++)
    {
        try
        {
            context.Database.Migrate();
            break; // Exit loop if successful
        }
        catch
        {
            // Log and wait before retrying
            await Task.Delay(2000); // wait 2 seconds
        }
    }
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
// Enable middleware to serve generated Swagger as a JSON endpoint
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});
app.Run();
