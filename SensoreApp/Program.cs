using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SensoreApp.Middleware;
using SensoreApp.Models;
using SensoreApp.Services;

var builder = WebApplication.CreateBuilder(args);

//
// 🧠 1️⃣ Structured Logging (Console + Debug)
//
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

//
// 🧩 2️⃣ Add MVC Controllers
//
builder.Services.AddControllersWithViews();

//
// 🗄️ 3️⃣ Configure Database Connection (EF Core)
//
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

//
// 🪵 3.5️⃣ Add Centralized Logging Service
//
builder.Services.AddScoped<ILoggingService, LoggingService>();

//
// 🌐 4️⃣ Enable CORS (Frontend Communication)
//
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

//
// 📘 5️⃣ Enhanced Swagger Documentation
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Pressure Monitoring API",
        Version = "v1",
        Description = "Unified backend API for Sensor Data, Authentication, Dashboard, and Alerts modules.",
        Contact = new OpenApiContact
        {
            Name = "SmartPressure Team",
            Email = "support@smartpressure.local",
            Url = new Uri("https://github.com/krishna1tawar/SmartPressureMonitoring")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Group endpoints by controller
    c.TagActionsBy(api =>
    {
        var controller = api.GroupName ?? api.ActionDescriptor.RouteValues["controller"];
        return new[] { controller ?? "Default" };
    });
    c.DocInclusionPredicate((_, _) => true);

    // Include XML comments (optional)
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

//
// 🧾 6️⃣ Add JSON Options
//
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.PropertyNamingPolicy = null;
});

//
// ✅ Build the App
//
var app = builder.Build();

//
// ⚙️ 7️⃣ Configure Middleware
//
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartPressureMonitoring API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//
// 🌐 8️⃣ Global Middleware
//
app.UseRequestResponseLogging();
app.UseGlobalExceptionMiddleware();   // centralized error handling
app.UseCors("AllowAll");              // global CORS
app.UseAuthorization();

//
// 🚀 9️⃣ Default Route
//
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();