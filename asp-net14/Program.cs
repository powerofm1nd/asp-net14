using System.Diagnostics;
using asp_net_13;
using Microsoft.AspNetCore.Diagnostics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

//Адресса для Open Telemetry Collector
var otelCollectorEndpoint = new Uri("http://127.0.0.1:4317");

//Налаштування для логування з OpenTelemetry
builder.Services.AddOpenTelemetry()
.WithTracing(tracingBuilder =>
{
    tracingBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
        
        .AddService("OpenTelemetryDemoService"))
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            
            //Додавання фільтру
            options.Filter = (httpContext) =>
            {
                var hasError = httpContext.Response.StatusCode >= 400;
                var exceptionFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
                var hasException = exceptionFeature?.Error != null;
                return hasError || hasException;
            };
        })
        .AddProcessor(new ActivityFilteringProcessor((activity) =>
        {
            // Відбираємо лише ті сліди, де тривалість більше 1 секунди
            var duration = (activity as Activity).Duration;
            return duration > TimeSpan.FromSeconds(1);
        }))
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddSource("OpenTelemetryDemo")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = otelCollectorEndpoint; // Підключення до OpenTelemetry Collector
        });
})
.WithMetrics(metricsBuilder =>
{
    metricsBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("OpenTelemetryDemoService"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = otelCollectorEndpoint;
        });
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();