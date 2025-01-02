using Concert.Business.Constants;
using Concert.Business.Mappings;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.API.Middlewares;
using Concert.DataAccess.API.Repositories;
using Concert.DataAccess.Interfaces;
using DotEnv.Core;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Environment variables management.

string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// Read environment variables.
new EnvLoader().Load();
var envVarReader = new EnvReader();

// Get connectionString.
string connectionString = string.Empty;

if (envName == Environments.Development)
{
    connectionString = envVarReader["DataBase_ConnectionString"];
}
else if (envName == Environments.Production)
{
    connectionString = Environment.GetEnvironmentVariable("DataBase_ConnectionString");
}

// Add services to the container.

builder.Services.AddControllers();

// Configure Cors (Cross-origin resource sharing) to allow API requests from the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });

    options.AddPolicy("AllowBlazorApp", builder =>
    {
        builder.WithOrigins("https://localhost:7079", "http://localhost:5155")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add the database service.
builder.Services.AddDbContext<ConcertDbContext>(options =>
    options.UseSqlServer(connectionString));

// Dependency injection for additional services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Automapper.
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// Add Serilog.
var logger = new LoggerConfiguration()
    .WriteTo.File(
        path: BackConstants.API_LOGS_FILE_FULL_PATH,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: BackConstants.API_LOGS_MAX_NUM_FILES)
    .MinimumLevel.Information()
    // To avoid logging irrelevant information from Microsoft
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    // To log as information from Microsoft only lifetime events
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

// Use the CORS policy
app.UseCors("AllowBlazorApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Concert API");
        options.WithTheme(ScalarTheme.Saturn);
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Middlewares
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RouteIdValidationMiddleware>();
app.UseMiddleware<ModelValidationMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();