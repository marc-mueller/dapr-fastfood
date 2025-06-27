using Microsoft.AspNetCore.SignalR;

namespace FrontendCustomerOrderStatus.Controllers
{
    public class OrderUpdateHub : Hub
    {
        private readonly ILogger<OrderUpdateHub> _logger;

        public OrderUpdateHub(ILogger<OrderUpdateHub> logger)
        {
            _logger = logger;
        }

        public async Task SubscribeToOrderUpdates()
        {
            var connectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(connectionId, Constants.HubGroupCustomerOrderStatusMonitors);
            _logger.LogInformation("Connection {ConnectionId} subscribed to kitchen work monitor group", connectionId);
        }

        public async Task UnsubscribeFromOrderUpdates()
        {
            var connectionId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(connectionId, Constants.HubGroupCustomerOrderStatusMonitors);
            _logger.LogInformation("Connection {ConnectionId} unsubscribed from kitchen work monitor group", connectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Connection {ConnectionId} disconnected", connectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}