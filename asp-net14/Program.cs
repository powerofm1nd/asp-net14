using asp_net_13;
using OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Створення експортера для кастомного бекенду
var customExporter = new CustomTraceExporter("http://backend-url.com/trace");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracingBuilder =>
    {
        tracingBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddProcessor(new BatchActivityExportProcessor(customExporter));
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Налаштування middleware
app.UseRouting();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();