using AutoMapper;
using Core.CoreSettings;
using Core.Utilities;
using DataAccess.Contexts;
using DataAccess.Repositories;
using DDOT.MPS.Auth.Api;
using DDOT.MPS.Auth.Api.CustomConfiguration;
using DDOT.MPS.Auth.Api.HealthChecks;
using DDOT.MPS.Auth.Api.Managers;
using DDOT.MPS.Auth.Api.Middlewares;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.EntityFrameworkCore;
using Model.OptionModels;
using Model.Services.UserManagement;

var builder = WebApplication.CreateBuilder(args);

// Add configuration for appsettings.QA.json
//builder.Configuration
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
//    .AddJsonFile("appsettings.QA.json", optional: true, reloadOnChange: true)
//    .AddEnvironmentVariables();

// DB service config
builder.Services.AddDbContext<MpsDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Automapper config
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Application Insights
builder.Services.AddApplicationInsightsTelemetry();

/*
 * Dependency Injection
 */

builder.Services.AddSingleton<ITelemetryInitializer, DotMpsTelemetryInitializer>();

// Repositories
builder.Services.AddScoped<IMpsUserRepository, MpsUserRepository>();
builder.Services.AddScoped<IMpsAuthManagementRepository, MpsAuthManagementRepository>();
builder.Services.AddScoped<IMpsAgencyRepository, MpsAgencyRepository>();
builder.Services.AddScoped<IMpsAgencyCategoryRepository, MpsAgencyCategoryRepository>();
builder.Services.AddScoped<IMpsDatagridStateRepository, MpsDatagridStateRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>(); 

// Managers
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IAgencyManager, AgencyManager>();
builder.Services.AddScoped<IDatagridStateManager, DatagridStateManager>();
builder.Services.AddScoped<IExternalUserService, ExternalUserService>();
builder.Services.AddScoped<IGraphApiAuthorizationCodeProvider, GraphApiAuthorizationCodeProvider>();
builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<IPermissionsManager, PermissionsManager>();
builder.Services.AddScoped<IExternalUserService, ExternalUserService>();
builder.Services.AddScoped<ILoginHistoryManager, LoginHistoryManager>();
builder.Services.AddScoped<IMpsLoginHistoryRepository, MpsLoginHistoryRepository>();
builder.Services.AddScoped<IAppUtils, AppUtils>();
builder.Services.AddScoped<IJwtUtils, JwtUtils>();

// Options
builder.Services.AddOptions<GraphApiCredentialOptionsForClient>()
    .Configure(options => builder.Configuration.GetSection("GraphApiCredentialOptionsForClient").Bind(options));
builder.Services.AddOptions<GraphApiCredentialOptionsForAdmin>()
    .Configure(options => builder.Configuration.GetSection("GraphApiCredentialOptionsForAdmin").Bind(options));
builder.Services.AddOptions<GlobalAppSettings>()
    .Configure(options => builder.Configuration.GetSection("GlobalAppSettings").Bind(options));

// Add authorization services
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Get the environment name
var environmentName = builder.Environment.EnvironmentName;
// Register the environment name as a singleton service
builder.Services.AddSingleton(environmentName);

// Register custom health check
builder.Services.AddHealthChecks().AddCheck<CustomHealthCheck>("custom_health_check");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

//var logger = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger<Program>();
//logger.LogInformation($" ----------------- Application has started in {environmentName} environment ----------------- ");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapHealthChecks("/api/v1/healthcheck");

// Handler Correlation Id
app.UseMiddleware<CorrelationIdMiddleware>();

// Global Error Handler
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
