using Concert.DataAccess.InMemory.Repositories;
using Concert.UIWasm;
using Concert.UIWasm.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7284/api/") });

// @todo to be removed
builder.Services.AddScoped<ISongRequestsRepository, SongRequestsIMRepository>();

builder.Services.AddScoped<IWebApiExecuter, WebApiExecuter>();

await builder.Build().RunAsync();