using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sensore_Project;
using Sensore_Project.Models;
using Sensore_Project.Repositories;
using Sensore_Project.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC Controllers + Views
builder.Services.AddControllersWithViews();

// EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repository
builder.Services.AddScoped<SensorDataRepository>();
builder.Services.AddScoped<RiskPredictionRepository>();
builder.Services.AddScoped<RiskPredictionService>();
builder.Services.AddScoped<AlertsRepository>();
// ✅ Register ML Anomaly Detection Service
builder.Services.AddScoped<AnomalyDetectionService>();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sensor API", Version = "v1" });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Enable Swagger UI (Development only or always if preferred)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sensor API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// API Controllers (SensorDataController)
app.MapControllers();

// MVC Default Route (HomeController)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();