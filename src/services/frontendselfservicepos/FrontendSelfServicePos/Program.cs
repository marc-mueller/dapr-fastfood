using System.Text.Json;
using System.Text.Json.Serialization;
using FastFood.Common.Settings;
using FastFood.Observability.Common;
using FinanceService.Observability;
using FrontendSelfServicePos.Controllers;

var builder = WebApplication.CreateBuilder(args);

var observabilityOptions = builder.Configuration.GetObservabilityOptions();
builder.Services.AddObservability<IFrontendSelfServicePosObservability, FrontendSelfServicePosObservability>(observabilityOptions, options => new FrontendSelfServicePosObservability(options.ServiceName, options.ServiceName));

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3900";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60900";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}")
    .UseJsonSerializationOptions(new JsonSerializerOptions().ConfigureJsonSerializerOptions()));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ConfigureJsonSerializerOptions();
    })
    .AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();

app.UseObservability(observabilityOptions);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapSubscribeHandler();
app.MapHub<OrderUpdateHub>("/orderupdatehub");

app.MapHealthChecks("/healthz");

app.Run();
