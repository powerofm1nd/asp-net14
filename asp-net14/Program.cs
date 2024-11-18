using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

//Адресса для Open Telemetry Collector
var otelCollectorEndpoint = new Uri("http://127.0.0.1:4317");

//Налаштування для логування з OpenTelemetry
var resourceBuilder = ResourceBuilder.CreateDefault().AddService(".Net Log Service");
builder.Logging.AddOpenTelemetry(logging => {
    logging.IncludeScopes = true;
    logging.SetResourceBuilder(resourceBuilder)
    .AddOtlpExporter(otlpOptions => {
        otlpOptions.Protocol = OtlpExportProtocol.Grpc;
        otlpOptions.Endpoint = otelCollectorEndpoint;
    });
});

var otel = builder.Services.AddOpenTelemetry();
otel.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

//Налаштування Metrics
otel.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddConsoleExporter());

//Налаштування Tracing
otel.WithTracing(tracing => {
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddOtlpExporter(otlpOptions => {
        otlpOptions.Endpoint = otelCollectorEndpoint;
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