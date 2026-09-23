using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.IO.Converters;
using Scalar.AspNetCore;
using SmartAudit.Api.Middleware;
using SmartAudit.Api.Services;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Import;
using SmartAudit.Api.Services.Reports;
using SmartAudit.Api.Services.Simulation;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Settings;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Chybí connection string Postgres");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings nebyli nalezeny");
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddSingleton(builder.Configuration.GetSection(OpenDataSettings.SectionName).Get<OpenDataSettings>() ?? new OpenDataSettings());
builder.Services.AddSingleton(builder.Configuration.GetSection(SimulationSettings.SectionName).Get<SimulationSettings>() ?? new SimulationSettings());
builder.Services.AddSingleton(builder.Configuration.GetSection(AuditSettings.SectionName).Get<AuditSettings>() ?? new AuditSettings());

builder.Services.AddScoped<ITokenService, TokenService>();

// AI analýza přes Azure OpenAI — bez klíče aplikace funguje, jen AI vrací „není nastavená“
builder.Services.AddSingleton(builder.Configuration.GetSection(AzureOpenAiSettings.SectionName).Get<AzureOpenAiSettings>() ?? new AzureOpenAiSettings());
builder.Services.AddScoped<IAiAuditService, AiAuditService>();

// Externí zdroje dat
builder.Services.AddHttpClient<IKhkOpenDataService, KhkOpenDataService>(c => c.Timeout = TimeSpan.FromSeconds(90));
builder.Services.AddHttpClient<ICuzkBuildingService, CuzkBuildingService>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IOpenMeteoService, OpenMeteoService>(c => c.Timeout = TimeSpan.FromSeconds(30));

// Import a příprava dat
builder.Services.AddSingleton<SnapshotStore>();
builder.Services.AddSingleton<IPipelineStateService, PipelineStateService>();
builder.Services.AddScoped<IOpenDataImportService, OpenDataImportService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IBuildingEnrichmentService, BuildingEnrichmentService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IDemoUserService, DemoUserService>();
builder.Services.AddHostedService<DataPipelineService>();

// Simulace telemetrie a audit
builder.Services.AddScoped<ISensorProvisioningService, SensorProvisioningService>();
builder.Services.AddScoped<ISensorDataSource, SimulatedSensorDataSource>();
builder.Services.AddScoped<IFacilityAuditService, FacilityAuditService>();
builder.Services.AddHostedService<SensorSimulationService>();

// PDF reporty — QuestPDF Community licence (zdarma pro malé organizace), písmo Hradec a logo HK
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
ReportStyles.Initialize(builder.Environment.ContentRootPath);
builder.Services.AddScoped<IReportAiSource, ReportAiSource>();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.Zero,

        RoleClaimType = AppClaims.Role
    };
});

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SmartAuditDbContext>(op => op.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// HTTPS řeší reverzní proxy / Next.js, API v kontejneru běží jen na HTTP

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
