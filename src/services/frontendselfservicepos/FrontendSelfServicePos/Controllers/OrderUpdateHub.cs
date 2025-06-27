using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FrontendSelfServicePos.Controllers
{
    public class OrderUpdateHub : Hub
    {
        private readonly ILogger<OrderUpdateHub> _logger;

        public OrderUpdateHub(ILogger<OrderUpdateHub> logger)
        {
            _logger = logger;
        }

        public async Task SubscribeToOrder(string orderId)
        {
            var connectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(connectionId, orderId);
            _logger.LogInformation("Connection {ConnectionId} subscribed to order {OrderId}", connectionId, orderId);
        }

        public async Task UnsubscribeFromOrder(string orderId)
        {
            var connectionId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(connectionId, orderId);
            _logger.LogInformation("Connection {ConnectionId} unsubscribed from order {OrderId}", connectionId, orderId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Connection {ConnectionId} disconnected", connectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}