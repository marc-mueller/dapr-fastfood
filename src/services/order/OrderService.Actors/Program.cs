using System.Text.Json;
using System.Text.Json.Serialization;
using FastFood.Common.Settings;
using FastFood.FeatureManagement.Common.Extensions;
using FastFood.Observability.Common;
using FinanceService.Observability;
using OrderPlacement.Actors;

var builder = WebApplication.CreateBuilder(args);

// Add Azure App Configuration if configured
var appConfigConfigured = builder.Configuration.AddAzureAppConfigurationIfConfigured(builder.Configuration);

var observabilityOptions = builder.Configuration.GetObservabilityOptions();
builder.Services.AddObservability<IOrderServiceActorObservability, OrderServiceActorObservability>(observabilityOptions, options => new OrderServiceActorObservability(options.ServiceName, options.ServiceName));

// Add Feature Management (with Azure App Configuration support)
builder.Services.AddObservableFeatureManagement();

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3650";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60650";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}")
    .UseJsonSerializationOptions(new JsonSerializerOptions().ConfigureJsonSerializerOptions()));

builder.Services.AddActors(options => { options.Actors.RegisterActor<OrderActor>(); });


builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.ConfigureJsonSerializerOptions(); })
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

app.MapActorsHandlers();

app.MapHealthChecks("/healthz");

app.Run();