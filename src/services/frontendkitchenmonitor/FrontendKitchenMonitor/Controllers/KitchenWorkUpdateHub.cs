using Microsoft.AspNetCore.SignalR;

namespace FrontendKitchenMonitor.Controllers;

public class KitchenWorkUpdateHub : Hub
{
    private readonly ILogger<KitchenWorkUpdateHub> _logger;

    public KitchenWorkUpdateHub(ILogger<KitchenWorkUpdateHub> logger)
    {
        _logger = logger;
    }
    
    public async Task SubscribeToWork()
    {
        var connectionId = Context.ConnectionId;
        await Groups.AddToGroupAsync(connectionId, Constants.HubGroupKitchenMonitors);
        _logger.LogInformation("Connection {ConnectionId} subscribed to kitchen work monitor group", connectionId);
    }

    public async Task UnsubscribeFromWork()
    {
        var connectionId = Context.ConnectionId;
        await Groups.RemoveFromGroupAsync(connectionId, Constants.HubGroupKitchenMonitors);
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from kitchen work monitor group", connectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Connection {ConnectionId} disconnected", connectionId);
        await base.OnDisconnectedAsync(exception);
    }
}