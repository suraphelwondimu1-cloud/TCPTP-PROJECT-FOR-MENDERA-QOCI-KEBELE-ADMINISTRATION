using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MenderaQociDFMS.Data;
using MenderaQociDFMS.Models;
using MenderaQociDFMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database Context Setup with MySQL and SQLite Fallback
var mysqlConnString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? "Server=localhost;Database=MenderaQociDFMS;User=root;Password=root;";
var sqliteConnString = builder.Configuration.GetConnectionString("SqliteConnection") 
                       ?? "Data Source=MenderaQociDFMS.db";
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "MySQL";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var serverVersion = ServerVersion.AutoDetect(mysqlConnString);
            options.UseMySql(mysqlConnString, serverVersion);
        }
        catch
        {
            // Fallback to SQLite if local MySQL service is not authenticated/accessible
            options.UseSqlite(sqliteConnString);
        }
    }
    else
    {
        options.UseSqlite(sqliteConnString);
    }
});

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings appropriate for student/kebele admin environment
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Register Custom Application Services
builder.Services.AddScoped<BackupService>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbInitializer.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
