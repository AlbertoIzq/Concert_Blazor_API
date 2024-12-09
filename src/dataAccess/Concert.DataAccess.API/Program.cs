using Concert.Business.Mappings;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.API.Repositories;
using Concert.DataAccess.Interfaces;
using DotEnv.Core;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add the database service.
builder.Services.AddDbContext<ConcertDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add IUnitOfWork service.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Automapper.
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

var app = builder.Build();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();