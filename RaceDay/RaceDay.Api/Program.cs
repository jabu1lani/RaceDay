using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. ADD CONTROLLERS
// ============================================================
builder.Services.AddControllers();

// ============================================================
// 2. ADD SWAGGER (minimal, no extra packages needed)
// ============================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// 3. DATABASE CONTEXT
// ============================================================
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 4. SESSION
// ============================================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// ============================================================
// 5. CUSTOM SERVICES
// ============================================================
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ISessionService, SessionService>();

var app = builder.Build();

// ============================================================
// 6. SWAGGER MIDDLEWARE
// ============================================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.Run();