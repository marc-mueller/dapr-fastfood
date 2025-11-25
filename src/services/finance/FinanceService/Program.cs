using System.Text.Json;
using System.Text.Json.Serialization;
using FastFood.Common.Settings;
using FastFood.Observability.Common;
using FinanceService.Observability;
using FinanceService.Services;
using FinanceService.Storage.Authentication;
using FinanceService.Storage.Extensions;
using FinanceService.Storage.Storages;
using FinanceService.Storage.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

AppContext.SetSwitch("Microsoft.AspNetCore.Mvc.ApiExplorer.IsEnhancedModelMetadataSupported", true);

var builder = WebApplication.CreateBuilder(args);

// Feature Management
builder.Services.AddFeatureManagement();

// Observability
var observabilityOptions = builder.Configuration.GetObservabilityOptions();
builder.Services.AddObservability<IFinanceServiceObservability, FinanceServiceObservability>(observabilityOptions, options => new FinanceServiceObservability(options.ServiceName, options.ServiceName));

// Dapr
var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3900";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60900";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}")
    .UseJsonSerializationOptions(new JsonSerializerOptions().ConfigureJsonSerializerOptions()));

// Database Configuration with Feature Flag
var serviceProvider = builder.Services.BuildServiceProvider();
var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();
var UseInMemoryDatabase = await featureManager.IsEnabledAsync("UseInMemoryDatabase");

if (UseInMemoryDatabase)
{
    builder.Services.AddDbContext<FinanceStorage>(options =>
        options.UseInMemoryDatabase("FinanceInMemoryDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("FinanceDatabase");
    builder.Services.AddDbContext<FinanceStorage>(options =>
    {
        options.UseSqlServer(connectionString);
        options.AddInterceptors(new AzureAuthenticationInterceptor());
    });
}

// Register repositories and services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderFinanceService, OrderFinanceService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

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

// Ensure database is created and optionally seeded (for demo purposes)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FinanceStorage>();
    await context.Database.EnsureCreatedAsync();
    
    // Seed demo data in development
    if (app.Environment.IsDevelopment())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Seeding demo data for development environment");
        await context.SeedDemoDataAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents();
app.UseObservability(observabilityOptions);
app.MapControllers();
app.MapSubscribeHandler();

app.MapHealthChecks("/healthz");

app.Run();