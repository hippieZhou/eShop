using System.Text.Json;
using System.Text.Json.Serialization;
using eShop.Api.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

builder.Services.AddDbContextFactory<BlogContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention(); ;
});

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