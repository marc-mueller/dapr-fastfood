using System.Text.Json.Serialization;
using Dapr.Workflow;
using OrderPlacement.Services;
using OrderPlacement.Storages;
using OrderPlacement.Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3600";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60600";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}"));

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
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

app.MapControllers();
app.MapSubscribeHandler();

app.MapHealthChecks("/healthz");

app.Run();