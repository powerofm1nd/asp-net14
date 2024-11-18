using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using asp_net_13.Models;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace asp_net_13.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Tracer _tracer;
    
    public HomeController(ILogger<HomeController> logger, TracerProvider tracerProvider)
    {
        _logger = logger;
        _tracer = tracerProvider.GetTracer("MyAspNetCoreTracer");
    }

    public IActionResult Index() {
        _logger.Log(LogLevel.Information, "!!! Something !!!");
        
        using (var activity = new ActivitySource("OpenTelemetryDemo")
        .StartActivity("SampleController.Get")) 
        {
            // Додавання атрибутів
            activity?.SetTag("http.method", "GET");
            activity?.SetTag("priority", "high");
            
            activity?.AddBaggage("priority", "high");
            
            // Додаємо атрибути з контексту запиту
            var requestId = HttpContext?.TraceIdentifier;
            activity?.SetTag("request_id", requestId);
            return View();
        }
    }
    
    public IActionResult NoPage()
    {
        return NotFound();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}