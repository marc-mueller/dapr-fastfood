using System.Text.Json;
using System.Text.Json.Serialization;
using Dapr.Workflow;
using FastFood.Common.Settings;
using FastFood.FeatureManagement.Common.Extensions;
using FastFood.Observability.Common;
using FinanceService.Observability;
using OrderPlacement.Services;
using OrderPlacement.Storages;
using OrderPlacement.Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add Azure App Configuration if configured
var appConfigConfigured = builder.Configuration.AddAzureAppConfigurationIfConfigured(builder.Configuration);

var observabilityOptions = builder.Configuration.GetObservabilityOptions();
builder.Services.AddObservability<IOrderServiceObservability, OrderServiceObservability>(observabilityOptions, options => new OrderServiceObservability(options.ServiceName, options.ServiceName));

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3600";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60600";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}")
    .UseJsonSerializationOptions(new JsonSerializerOptions().ConfigureJsonSerializerOptions()));

// Add feature management
builder.Services.AddObservableFeatureManagement();

// Add pricing service
builder.Services.AddSingleton<IOrderPricingService, OrderPricingService>();

// Add Azure App Configuration refresh middleware if configured
if (appConfigConfigured)
{
    builder.Services.AddAzureAppConfiguration();
}

builder.Services.AddSingleton<IOrderEventRouter, OrderEventRouter>();
builder.Services.AddSingleton<IOrderProcessingServiceActor, OrderProcessingServiceActor>();
builder.Services.AddSingleton<IOrderProcessingServiceState, OrderProcessingServiceState>();
builder.Services.AddSingleton<IOrderProcessingServiceWorkflow, OrderProcessingServiceWorkflow>();

builder.Services.AddDaprWorkflow(options =>
{
    options.RegisterWorkflow<OrderProcessingWorkflow>();
    options.RegisterActivity<CreateOrderActivity>();
    options.RegisterActivity<AssignCustomerActivity>();
    options.RegisterActivity<AssignInvoiceAddressActivity>();
    options.RegisterActivity<AssignDeliveryAddressActivity>();
    options.RegisterActivity<AddItemActivity>();
    options.RegisterActivity<RemoveItemActivity>();
    options.RegisterActivity<ConfirmOrderActivity>();
    options.RegisterActivity<ConfirmPaymentActivity>();
    options.RegisterActivity<StartProcessingActivity>();
    options.RegisterActivity<ItemFinishedActivity>();
    options.RegisterActivity<OrderServedActivity>();
});

builder.Services.AddSingleton<IOrderStorage, OrderStorage>();

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