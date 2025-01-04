using Blazored.Toast;
using Concert.UIWasm;
using Concert.UIWasm.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add services to the container.

// Add HttpClient.
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7284/api/") });

// Dependency injection for additional services
builder.Services.AddScoped<IWebApiExecuter, WebApiExecuter>();

// Add toast component.
builder.Services.AddBlazoredToast();

// Add Serilog.
Log.Logger = new LoggerConfiguration()
    .WriteTo.BrowserConsole()
    .MinimumLevel.Information()
    // To avoid logging irrelevant information from Microsoft
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    // To log as information from Microsoft only lifetime events
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .CreateLogger();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders(); // Remove default logging providers
    logging.AddProvider(new SerilogLoggerProvider(Log.Logger));
});

await builder.Build().RunAsync();