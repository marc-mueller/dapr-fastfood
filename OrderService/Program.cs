using System.Text.Json.Serialization;
using OrderPlacement.Services;
using OrderPlacement.Storages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3600";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60600";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}"));

var useActors = true;
if (useActors)
{
    builder.Services.AddSingleton<IOrderProcessingService, OrderProcessingServiceActor>();
}
else
{
    builder.Services.AddSingleton<IOrderProcessingService, OrderProcessingServiceState>();
}

builder.Services.AddSingleton<IReadStorage, ReadStorage>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddDapr();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();