using System.Text.Json;
using System.Text.Json.Serialization;
using FastFood.Common.Settings;
using FastFood.FeatureManagement.Common.Extensions;
using FastFood.Observability.Common;
using FinanceService.Observability;
using KitchenService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Azure App Configuration if configured
var appConfigConfigured = builder.Configuration.AddAzureAppConfigurationIfConfigured(builder.Configuration);

var observabilityOptions = builder.Configuration.GetObservabilityOptions();
builder.Services.AddObservability<IKitchenServiceObservability, KitchenServiceObservability>(observabilityOptions, options => new KitchenServiceObservability(options.ServiceName, options.ServiceName));

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3700";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60700";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}")
    .UseJsonSerializationOptions(new JsonSerializerOptions().ConfigureJsonSerializerOptions()));

// Add feature management
builder.Services.AddObservableFeatureManagement();

// Add Azure App Configuration refresh middleware if configured
if (appConfigConfigured)
{
    builder.Services.AddAzureAppConfiguration();
}

builder.Services.AddSingleton<IKitchenService, KitchenService.Services.KitchenService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ConfigureJsonSerializerOptions();
    })
    .AddDapr();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();

// Add Azure App Configuration refresh middleware if configured
if (appConfigConfigured)
{
    app.UseAzureAppConfiguration();
}

app.UseObservability(observabilityOptions);

app.MapControllers();
app.MapSubscribeHandler();

app.MapHealthChecks("/healthz");

app.Run();