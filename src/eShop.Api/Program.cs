using Asp.Versioning;
using eShop.Api.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NSwag;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region API Routing
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    o.SerializerOptions.NumberHandling |= JsonNumberHandling.AllowNamedFloatingPointLiterals;
});
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    o.JsonSerializerOptions.NumberHandling |= JsonNumberHandling.AllowNamedFloatingPointLiterals;
});
#endregion

#region OpenAPI
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
});
builder.Services.AddOpenApiDocument(options => {
    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1",
            Title = "eShop API",
            Description = "An ASP.NET Core Web API for managing eShop items",
            TermsOfService = "https://example.com/terms",
            Contact = new OpenApiContact
            {
                Name = "Example Contact",
                Url = "https://example.com/contact"
            },
            License = new OpenApiLicense
            {
                Name = "Example License",
                Url = "https://example.com/license"
            }
        };
    };
});
#endregion

#region ApiVersioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddMvc() // This is needed for controllers
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});
#endregion

#region EF Core

builder.Services.AddDbContextFactory<BlogContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Database");
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention(); ;
});

#endregion

#region OpenTelemetry 
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
    }).SetResourceBuilder(
        ResourceBuilder.CreateDefault()
        .AddService("eShop.Api"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(metrics => metrics.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:4317");
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
    }).SetResourceBuilder(
        ResourceBuilder.CreateDefault()
        .AddService("eShop.Api"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

// set OpenTelemetry to log to console at Warning level and above
//builder.Logging.AddFilter<OpenTelemetryLoggerProvider>(null, LogLevel.Warning);
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;

    // we can implement a custom exporter if needed https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Console/ConsoleLogRecordExporter.cs
    options.AddConsoleExporter()
    .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService("eShop.Api")
        .AddAttributes(new Dictionary<string, object>
        {
            { "service.name", "eShop.Api" },
            { "service.environment", builder.Environment.EnvironmentName },
            { "service.version", Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "1.0.0" },
            { "host.name", Environment.MachineName },
            { "host.architecture", Environment.Is64BitOperatingSystem ? "x64" : "x86" }
        }));
});
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "openapi/v1.json";
    });
    app.UseReDoc(options =>
    {
        options.DocumentPath = "openapi/v1.json";
        options.Path = "/redoc";
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program
{
    // This partial class is used to allow the Program class to be extended in tests.
    // It can be used to add additional configuration or services for testing purposes.
    // For example, you can add a test database or mock services here.
}