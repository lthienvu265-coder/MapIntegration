using IndoorWayfinder.Api.Data;
using IndoorWayfinder.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// DATABASE (PostgreSQL)
// ==========================

var envUrl = Environment.GetEnvironmentVariable("WAYFINDER_DB_URL")
             ?? Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrWhiteSpace(envUrl))
{
    // If full PostgreSQL URL provided
    // Example:
    // postgres://user:pass@localhost:5432/dbname
    connectionString = ConvertPostgresUrl(envUrl);
}
else
{
    connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=wayfinder;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<WayfinderContext>(opt =>
    opt.UseNpgsql(connectionString));

// ==========================
// SERVICES
// ==========================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RouteService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ==========================
// AUTO MIGRATION (like create_all)
// ==========================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WayfinderContext>();
    db.Database.Migrate(); // IMPORTANT for PostgreSQL
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DevCors");

// ==========================
// STATIC FILES
// ==========================

var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
Directory.CreateDirectory(dataPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(dataPath),
    RequestPath = "/static"
});

var appPath = Path.Combine(Directory.GetCurrentDirectory(), "frontend");
if (Directory.Exists(appPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(appPath),
        RequestPath = "/app"
    });
}

app.MapControllers();

app.MapGet("/health", () => Results.Json(new { status = "ok" }));

app.Run();


// ==========================
// HELPER: Convert postgres:// URL
// ==========================

static string ConvertPostgresUrl(string url)
{
    var uri = new Uri(url);

    var userInfo = uri.UserInfo.Split(':', StringSplitOptions.RemoveEmptyEntries);

    var username = userInfo[0];
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    var db = uri.AbsolutePath.TrimStart('/');

    return $"Host={uri.Host};Port={uri.Port};Database={db};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}