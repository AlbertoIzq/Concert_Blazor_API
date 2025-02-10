using Blazored.Modal;
using Blazored.Toast;
using Concert.UIWasm;
using Concert.UIWasm.Data;
using Concert.UIWasm.Services;
using Microsoft.AspNetCore.Components.Authorization;
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
builder.Services.AddScoped(sp => new HttpClient(new CookieHandler())
{
    BaseAddress = new Uri("https://localhost:7284/api/"),
    DefaultRequestHeaders =
    {
        { "X-Requested-With", "XMLHttpRequest" } // Helps prevent CSRF issues
    }
});

// Dependency injection for additional services
builder.Services.AddScoped<IWebApiExecuter, WebApiExecuter>();

// Add components.
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();

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

// Enable authentication
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();