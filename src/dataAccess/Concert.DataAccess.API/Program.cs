using Concert.Business.Constants;
using Concert.Business.Mappings;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.API.Helpers;
using Concert.DataAccess.API.Middlewares;
using Concert.DataAccess.API.Repositories;
using Concert.DataAccess.Interfaces;
using DotEnv.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Environment variables management.
string connectionString = string.Empty;
string connectionStringAuth = string.Empty;

string adminUserName = string.Empty;
string adminUserPassword = string.Empty;

string jwtSecretKey = string.Empty;
string jwtIssuer = string.Empty;
string jwtAudience = string.Empty;

string encryptionSecretKey = string.Empty;

ReadEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();

// Configure Cors (Cross-origin resource sharing) to allow API requests from the frontend.
ConfigureCorsPolicies();

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
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddSingleton<IEncryptionService>(provider =>
    new AesEncryptionService(encryptionSecretKey));

// Add Automapper.
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// AddIdentityAndConfigurePassword.
AddIdentityAndConfigurePassword();

// AddJwtAuthentication.
AddJwtAndCookieAuthentication();

// Add Serilog.
AddSerilog();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Run database migrations automatically and seed Admin user.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ConcertAuthDbContext>();

    dbContext.Database.Migrate(); // Ensures database is up-to-date

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await SeedAdminUser(userManager, roleManager, adminUserName, adminUserPassword);
}

// Use the CORS policy.
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Middlewares.
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<RouteIdValidationMiddleware>();
app.UseMiddleware<ModelValidationMiddleware>();

app.MapControllers();

app.Run();

#region Helpers

void ReadEnvironmentVariables()
{
    string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    // Read environment variables.
    new EnvLoader().Load();
    var envVarReader = new EnvReader();

    // Get connection strings.
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

    // Get Admin user parameters.
    adminUserName = envVarReader["DatabaseAuth_AdminUser_Email"];
    adminUserPassword = envVarReader["DatabaseAuth_AdminUser_Password"];

    // Get JWT parameters.
    jwtSecretKey = envVarReader["Jwt_SecretKey"];
    jwtIssuer = envVarReader["Jwt_Issuer"];
    jwtAudience = envVarReader["Jwt_Audience"];

    // Get encryption parameter.
    encryptionSecretKey = envVarReader["Encryption_SecretKey"];
}

void ConfigureCorsPolicies()
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
                   
        });

        options.AddPolicy("AllowBlazorApp", builder =>
        {
            builder.WithOrigins("https://localhost:7079", "http://localhost:5155")
                   .AllowCredentials() // Allow cookies
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
    });
}

void AddIdentityAndConfigurePassword()
{
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
}

void AddJwtAndCookieAuthentication()
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Use cookies first
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // JWT token as fallback
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Challenge with JWT token
    })
    .AddCookie("AuthCookie", options =>
    {
        options.Cookie.HttpOnly = true; // Prevents JavaScript access (XSS protection)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requires HTTPS
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.Name = BackConstants.JWT_TOKEN_COOKIE_NAME;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(BackConstants.ACCESS_TOKEN_EXPIRATION_MINUTES);
        options.LoginPath = "/login"; // Redirect path if unauthenticated
        options.LogoutPath = "/logout"; // Logout path
        options.AccessDeniedPath = "/access-denied";
    })
    .AddCookie("RefreshCookie", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = BackConstants.REFRESH_TOKEN_COOKIE_NAME;
        options.ExpireTimeSpan = TimeSpan.FromHours(BackConstants.REFRESH_TOKEN_EXPIRATION_HOURS);
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudiences = new[] { jwtAudience },
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
        options.Events = new JwtBearerEvents
        {
            // Used to be able to read the JWT token from the cookie
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey(BackConstants.JWT_TOKEN_COOKIE_NAME))
                {
                    context.Token = context.Request.Cookies[BackConstants.JWT_TOKEN_COOKIE_NAME];
                }
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                // Suppress the default challenge behavior so there's not an exception
                // because the response has already started.
                context.HandleResponse();

                // Log Unauthorized request details.
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                LoggerHelper<Program>.LogUnauthorizedRequest(logger, context.HttpContext);

                // Customize response.
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "You aren't authorized to access this resource.",
                };

                if (context.Error is not null && context.ErrorDescription is not null)
                {
                    problemDetails.Detail = $"Error: {context.Error}, ErrorDescription: {context.ErrorDescription}";
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(problemDetails);
            },
            OnForbidden = async context =>
            {
                // Log Forbidden request details.
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                LoggerHelper<Program>.LogForbiddenRequest(logger, context.HttpContext);

                // Customize response.
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "You don't have permission to access this resource."
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        };
    });
}

void AddSerilog()
{
    var logger = new LoggerConfiguration()
    .WriteTo.File(
        path: BackConstants.API_LOGS_FILE_FULL_PATH,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: BackConstants.API_LOGS_MAX_NUM_FILES)
    .MinimumLevel.Information()
    // To avoid logging irrelevant information from Microsoft.
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    // To log as information from Microsoft only lifetime events.
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .CreateLogger();
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(logger);
}

#endregion

#region DatabaseHelpers

async Task SeedAdminUser(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
    string adminName, string adminPassword)
{
    var adminUser = await userManager.FindByEmailAsync(adminName);
    if (adminUser is null)
    {
        adminUser = AdminUserIniData(adminName);

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, BackConstants.ADMIN_ROLE_NAME);
            await userManager.AddToRoleAsync(adminUser, BackConstants.READER_ROLE_NAME);
            await userManager.AddToRoleAsync(adminUser, BackConstants.WRITER_ROLE_NAME);
        }
    }
}

IdentityUser AdminUserIniData(string userName)
{
    var _adminUser = new IdentityUser()
    {
        Id = BackConstants.ADMIN_USER_ID,
        UserName = userName,
        NormalizedUserName = userName.ToUpper(),
        Email = userName,
        NormalizedEmail = userName.ToUpper(),
        EmailConfirmed = true,
        SecurityStamp = Guid.NewGuid().ToString(),
        ConcurrencyStamp = Guid.NewGuid().ToString(),
    };
    return _adminUser;
}

#endregion