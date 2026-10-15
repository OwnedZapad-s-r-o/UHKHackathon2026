using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NetTopologySuite.IO.Converters;
using Scalar.AspNetCore;
using SmartAudit.Api.Controllers;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Middleware;
using SmartAudit.Api.Services;
using SmartAudit.Api.Services.Audit;
using SmartAudit.Api.Services.Contracts;
using SmartAudit.Api.Services.Guide;
using SmartAudit.Api.Services.Import;
using SmartAudit.Api.Services.Maintenance;
using SmartAudit.Api.Services.OpenData;
using SmartAudit.Api.Services.Reports;
using SmartAudit.Api.Services.Security;
using SmartAudit.Api.Services.Sensors;
using SmartAudit.Api.Services.Simulation;
using SmartAudit.Core.Common;
using SmartAudit.Core.Data;
using SmartAudit.Core.Domain.Helpers;
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

// AI analýza přes Azure OpenAI nebo OpenAI — bez klíče aplikace funguje, jen AI vrací „není nastavená“
builder.Services.AddSingleton(builder.Configuration.GetSection(AzureOpenAiSettings.SectionName).Get<AzureOpenAiSettings>() ?? new AzureOpenAiSettings());
builder.Services.AddSingleton(builder.Configuration.GetSection(OpenAiSettings.SectionName).Get<OpenAiSettings>() ?? new OpenAiSettings());
builder.Services.AddSingleton<IAiClientFactory, AiClientFactory>();
builder.Services.AddScoped<IAiAuditService, AiAuditService>();

// Průvodce (RAG): znalosti v paměti (embeddingy se spočítají při prvním dotazu), nástroje nad daty, záložní režim bez modelu
builder.Services.AddSingleton<IGuideKnowledgeBase, GuideKnowledgeBase>();
builder.Services.AddScoped<IGuideDataService, GuideDataService>();
builder.Services.AddScoped<IGuideAgent, GuideAgent>();
builder.Services.AddScoped<IGuideFallback, GuideFallback>();
builder.Services.AddScoped<IGuideChartBuilder, GuideChartBuilder>();

// Externí zdroje dat
builder.Services.AddHttpClient<IKhkOpenDataService, KhkOpenDataService>(c => c.Timeout = TimeSpan.FromSeconds(90));
builder.Services.AddHttpClient<ICuzkBuildingService, CuzkBuildingService>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IOpenMeteoService, OpenMeteoService>(c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient<IRuianGeocodingService, RuianGeocodingService>(c => c.Timeout = TimeSpan.FromSeconds(10));

// Přidání budovy správcem — místo v mapě se dohledá v datech, která aplikace už má (hranice, katastr, památky)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPlaceLookupService, PlaceLookupService>();

// Import a příprava dat
builder.Services.AddSingleton<SnapshotStore>();
builder.Services.AddSingleton<IPipelineStateService, PipelineStateService>();
builder.Services.AddScoped<IOpenDataImportService, OpenDataImportService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IBuildingEnrichmentService, BuildingEnrichmentService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IDemoUserService, DemoUserService>();
builder.Services.AddHostedService<DataPipelineService>();

// Registr smluv (DIA) — vyhledávání drží stav v PHP session, cookie si ContractRegisterService spravuje sám
builder.Services.AddSingleton(builder.Configuration.GetSection(ContractRegistrySettings.SectionName).Get<ContractRegistrySettings>() ?? new ContractRegistrySettings());
builder.Services.AddHttpClient<IContractRegisterService, ContractRegisterService>(c =>
    {
        c.BaseAddress = new Uri(ContractRegistryParser.BaseUrl);
        c.Timeout = TimeSpan.FromSeconds(60);
        c.DefaultRequestHeaders.UserAgent.ParseAdd("SmartAudit-KHK/1.0");
    })
    // Krátce žijící spojení — nečinné spojení přes NAT Dockeru může potichu umřít a čtení z něj pak visí
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false, PooledConnectionIdleTimeout = TimeSpan.FromSeconds(10), PooledConnectionLifetime = TimeSpan.FromMinutes(1) });
builder.Services.AddScoped<IContractSyncService, ContractSyncService>();

// Simulace telemetrie a audit
builder.Services.AddScoped<ISensorProvisioningService, SensorProvisioningService>();
builder.Services.AddScoped<ISensorDataSource, SimulatedSensorDataSource>();
builder.Services.AddScoped<IFacilityAuditService, FacilityAuditService>();
builder.Services.AddScoped<IPreAuditService, PreAuditService>();
builder.Services.AddSingleton<OpenDataLinks>();
builder.Services.AddSingleton<OpenDataSyncState>();
// Živá kontrola zdrojů: 5 s na zdroj hlídá CancellationToken ve službě, klient má rezervu
builder.Services.AddHttpClient<IOpenDataSyncService, OpenDataSyncService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(8);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("SmartAudit-KHK/1.0 (+https://www.datakhk.cz)");
});
builder.Services.AddSingleton<IAuditTrigger, AuditTrigger>();
builder.Services.AddHostedService<SensorSimulationService>();

// Měsíční oddíly tabulky měření — zakládá je dopředu
builder.Services.AddHostedService<ReadingPartitionService>();

// Skutečná čidla — IQRF Gateway Daemon přes MQTT broker (služba mqtt v docker-compose)
var iqrfSettings = builder.Configuration.GetSection(IqrfSettings.SectionName).Get<IqrfSettings>() ?? new IqrfSettings();
builder.Services.AddSingleton(iqrfSettings);
builder.Services.AddSingleton<IIqrfGatewayState, IqrfGatewayState>();
builder.Services.AddSingleton<IIqrfCommandBus, IqrfCommandBus>();
builder.Services.AddSingleton<IPairingRegistry, PairingRegistry>();
builder.Services.AddScoped<ISensorIngestService, SensorIngestService>();
builder.Services.AddHostedService<IqrfGatewayService>();

// Virtuální gatewaye (testovací síť, demo bez hardwaru) — odpovídají přes MQTT jako skutečný IQRF Gateway Daemon
builder.Services.AddScoped<IVirtualGatewayProvisioner, VirtualGatewayProvisioner>();
builder.Services.AddHostedService<VirtualGatewayService>();

// PDF reporty — QuestPDF Community licence (zdarma pro malé organizace), písmo Hradec a logo HK
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
ReportStyles.Initialize(builder.Environment.ContentRootPath);
builder.Services.AddScoped<IReportAiSource, ReportAiSource>();
builder.Services.AddScoped<IDistrictMapSource, DistrictMapSource>();

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

// Mimo Development žádná ukázková tajemství (JWT, MQTT, databáze) — raději nenastartovat
SecretsGuard.EnsureProductionSecrets(builder.Environment, jwtSettings, iqrfSettings, connectionString);

// AI analýza volá placený model — omezení počtu na uživatele (a mezipaměť výsledku v handleru) chrání rozpočet Azure OpenAI
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AiAuditController.RateLimitPolicy, context =>
    {
        var partition = context.User.TryGetUserId(out var userId) ? $"user:{userId}" : $"ip:{context.Connection.RemoteIpAddress}";

        return RateLimitPartition.GetFixedWindowLimiter(partition, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = AiAuditController.RateLimitRequests,
            Window = AiAuditController.RateLimitWindow,
            QueueLimit = 0,
        });
    });
    // Průvodce: krátké okno na uživatele (nebo IP adresu veřejnosti) — chrání rozpočet modelu před zahlcením.
    // Veřejnost chodí přes server Next.js — skutečnou adresu návštěvníka posílá v X-Forwarded-For (API je dostupné jen z tohoto počítače)
    options.AddPolicy(GuideController.RateLimitPolicy, context =>
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
        var ip = forwarded.Length > 0 ? forwarded : context.Connection.RemoteIpAddress?.ToString();
        var partition = context.User.TryGetUserId(out var userId) ? $"user:{userId}" : $"ip:{ip}";

        return RateLimitPartition.GetFixedWindowLimiter(partition, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = GuideController.RateLimitRequests,
            Window = GuideController.RateLimitWindow,
            QueueLimit = 0,
        });
    });
    options.OnRejected = async (context, ct) =>
    {
        if (context.HttpContext.Request.Path.StartsWithSegments("/api/guide"))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)GuideController.RateLimitWindow.TotalSeconds).ToString();
            await context.HttpContext.Response.WriteAsJsonAsync(new { Message = "Průvodce dostal moc dotazů najednou. Zkuste to prosím znovu za minutu." }, ct);
            return;
        }

        var minutes = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) ? (int)Math.Ceiling(retryAfter.TotalMinutes) : (int)AiAuditController.RateLimitWindow.TotalMinutes;
        context.HttpContext.Response.Headers.RetryAfter = (minutes * 60).ToString();
        await context.HttpContext.Response.WriteAsJsonAsync(new { Message = $"AI analýzu lze spustit nejvýš {AiAuditController.RateLimitRequests}× za {AiAuditController.RateLimitWindow.TotalMinutes:0} minut. Zkuste to znovu za {minutes} min." }, ct);
    };
});

// Otevřené API: CORS pro libovolný web, cache odpovědí na serveru (půdorys 2 dny, ostatní 5 minut)
builder.Services.AddCors(options => options.AddPolicy(OpenDataController.CorsPolicy, policy => policy.AllowAnyOrigin().WithMethods("GET").AllowAnyHeader()));
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy(OpenDataController.ListCachePolicy, policy => policy.Expire(TimeSpan.FromSeconds(OpenDataController.ListCacheSeconds)));
    options.AddPolicy(OpenDataController.FootprintCachePolicy, policy => policy.Expire(TimeSpan.FromSeconds(OpenDataController.FootprintCacheSeconds)));
    options.AddPolicy(MapController.RegionsCachePolicy, policy => policy.Expire(TimeSpan.FromSeconds(MapController.RegionsCacheSeconds)));
});

// Dokumentace: otevřené API je veřejná vždy, interní API aplikace jen při vývoji
builder.Services.AddOpenApi(OpenDataController.DocumentName, options =>
{
    // Jinak by dokument „v1“ převzal i všechny endpointy bez skupiny (interní API aplikace)
    options.ShouldInclude = description => description.GroupName == OpenDataController.DocumentName;
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "SmartAudit — otevřená data",
            Version = "v1",
            Description = "Objekty, které zřizuje Královéhradecký kraj: poloha, budova z ČÚZK, stav energetického auditu "
                + "a měření ze senzorů (zatím simulovaná). Bez přihlášení a bez klíče, jen čtení (GET). "
                + "Částky v Kč API neobsahuje.",
            License = new OpenApiLicense { Name = "CC BY 4.0", Url = new Uri("https://creativecommons.org/licenses/by/4.0/deed.cs") },
        };
        // Relativní adresa — dokumentace funguje přes proxy Next.js i přímo na API
        document.Servers = [new OpenApiServer { Url = "/" }];

        return Task.CompletedTask;
    });
});

if (builder.Environment.IsDevelopment())
    builder.Services.AddOpenApi("internal");

builder.Services.AddDbContext<SmartAuditDbContext>(op => op.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi("/api/open/{documentName}/openapi.json");
app.MapScalarApiReference("/api/open/docs", options =>
{
    options.WithTitle("SmartAudit — otevřené API")
        .WithOpenApiRoutePattern("/api/open/{documentName}/openapi.json")
        .AddDocument(OpenDataController.DocumentName, "Otevřená data v1", isDefault: true);

    if (app.Environment.IsDevelopment())
        options.AddDocument("internal", "Interní API aplikace");
});

// HTTPS řeší reverzní proxy / Next.js, API v kontejneru běží jen na HTTP

app.UseCors();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
