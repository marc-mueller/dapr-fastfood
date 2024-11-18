using System.Text.Json.Serialization;
using OrderPlacement.Actors;

var builder = WebApplication.CreateBuilder(args);


var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3650";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60650";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}"));

builder.Services.AddActors(options => { options.Actors.RegisterActor<OrderActor>(); });


builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); })
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

app.MapActorsHandlers();

app.MapHealthChecks("/healthz");

app.Run();