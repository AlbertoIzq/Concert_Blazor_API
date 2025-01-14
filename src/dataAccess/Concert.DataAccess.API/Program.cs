using Concert.Business.Constants;
using Concert.Business.Mappings;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.API.Middlewares;
using Concert.DataAccess.API.Repositories;
using Concert.DataAccess.Interfaces;
using DotEnv.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Environment variables management.

string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// Read environment variables.
new EnvLoader().Load();
var envVarReader = new EnvReader();

// Get connection strings
string connectionString = string.Empty;
string connectionStringAuth = string.Empty;

if (envName == Environments.Development)
{
    connectionString = envVarReader["DataBase_ConnectionString"];
    connectionStringAuth = envVarReader["DataBaseAuth_ConnectionString"];
}
else if (envName == Environments.Production)
{
    connectionString = Environment.GetEnvironmentVariable("DataBase_ConnectionString");
    connectionStringAuth = Environment.GetEnvironmentVariable("DataBaseAuth_ConnectionString");
}

// Get JWT parameters
string jwtSecretKey = envVarReader["Jwt_SecretKey"];
string jwtIssuer = envVarReader["Jwt_Issuer"];
string jwtAudience = envVarReader["Jwt_Audience"];

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

// Add the database services.
builder.Services.AddDbContext<ConcertDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<ConcertAuthDbContext>(options =>
    options.UseSqlServer(connectionStringAuth));

// Dependency injection for additional services.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

// Add Automapper.
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// Add Identity.
builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("Concert")
    .AddEntityFrameworkStores<ConcertAuthDbContext>()
    .AddDefaultTokenProviders(); // Used to generate tokens to reset passwords, change emails, etc.
// Configure password settings.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 10;
    options.Password.RequiredUniqueChars = 1;
});

// Add JWT Authentication.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudiences = new[] { jwtAudience },
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
    });

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();