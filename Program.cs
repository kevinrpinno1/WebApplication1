using Data;
using Data.Seed;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Configuration;
using WebApplication1.Data.Seed;
using WebApplication1.Services;
using WebApplication1.Validators;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Add services to the container

// -------------------------------------------------------------------
// binding app settings to classes for DI
// keeping only the required information in each class for security and clarity
// creating singleton services as these settings shouldn't change during app runtime
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

var constants = new Constants();
builder.Configuration.GetSection("Constants").Bind(constants);
builder.Services.AddSingleton(constants);
// --------------------------------------------------------------------

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();

// --------------------------------------------------------------------
// Db Context and Identity setup
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// for a deployed application these could be set in configuration securely and not hard-coded in case of cybersecurity policy updates
// for instance in GitHub Actions env secrets / variables 
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    // password settings
    options.Password.RequireDigit = true; 
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // user settings
    options.User.RequireUniqueEmail = true;

    // lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager<SignInManager<IdentityUser>>();


// --------------------------------------------------------------------

builder.Services.AddAuthentication(k=>
{
    k.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    k.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    k.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key)),
        RoleClaimType = ClaimTypes.Role 
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; //camelCase for JSON standard
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // ignore unless null value checks needed
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)); // if enums used
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // prevents JsonExceptions 
            if(builder.Environment.IsDevelopment()) options.JsonSerializerOptions.WriteIndented = true; // more readable in dev
        });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(cfg => { }, typeof(Program));
builder.Services.AddSwaggerGen();

// health checks added and mapped below - not fully required in this scenario 
// but useful for future deployment (and is a typical setup addition for me)
builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>(tags: ["ready"]) // checks database connectivity
                .AddCheck("self", () => HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/" + constants.ApiName.ToLower() + "/swagger.json", constants.ApiName);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Readiness probe, checks if the app is ready to receive traffic
app.MapHealthChecks("/health/readiness", new HealthCheckOptions
{
    AllowCachingResponses = true, // more performance expensive check, can be cached, short duration can be used instead
    Predicate = (check) => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Liveness probe, checks if the app is running
app.MapHealthChecks("/health/liveness", new HealthCheckOptions
{
    AllowCachingResponses = false, // don't cache response, need to know app is running at all times
    Predicate = (check) => check.Tags.Count == 0,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// ensures db is created and up to do with code-first models and latest migrations 
// short lived db context created for this, disposed properly after use
// do only dev, ci/cd scripts for prod to do migrations 
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await DemoIdentityUserSeeder.SeedDemoUsersAsync(userManager, roleManager, constants);

            await DemoDataSeeder.SeedDemoDataAsync(dbContext);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

await app.RunAsync();
