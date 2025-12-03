using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sensore_Project;
using Sensore_Project.Repositories;
using Sensore_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------
// MVC + Controllers
// ------------------------------
builder.Services.AddControllersWithViews();

// ------------------------------
// EF Core Database
// ------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------------------
// Dependency Injection (Interfaces → Implementations)
// ------------------------------
builder.Services.AddScoped<ISensorDataRepository, SensorDataRepository>();
builder.Services.AddScoped<IPressureMapRepository, PressureMapRepository>();
builder.Services.AddScoped<IAlertsRepository, AlertsRepository>();
builder.Services.AddScoped<IRiskPredictionRepository, RiskPredictionRepository>();

builder.Services.AddSingleton<IAnomalyDetectionService, AnomalyDetectionService>();
builder.Services.AddSingleton<IPressureMapAnalysisService, PressureMapAnalysisService>();
builder.Services.AddScoped<IRiskPredictionService, RiskPredictionService>();

// ------------------------------
// Swagger
// ------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sensor API",
        Version = "v1",
        Description = "Smart Pressure Monitoring API"
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ------------------------------
// Swagger UI
// ------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ------------------------------
// Middleware Pipeline
// ------------------------------
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// ------------------------------
// Controllers
// ------------------------------
app.MapControllers();

// ------------------------------
// Default MVC Route (Home page)
// ------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();