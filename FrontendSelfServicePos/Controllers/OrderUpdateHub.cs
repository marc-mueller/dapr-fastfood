using Microsoft.AspNetCore.SignalR;

namespace FrontendSelfServicePos.Controllers;

public class OrderUpdateHub : Hub
{
    private static readonly Dictionary<string, HashSet<string>> UserOrderSubscriptions = new();

    public async Task SubscribeToOrder(string orderId)
    {
        var connectionId = Context.ConnectionId;

        lock (UserOrderSubscriptions)
        {
            if (!UserOrderSubscriptions.ContainsKey(orderId))
                UserOrderSubscriptions[orderId] = new HashSet<string>();

            UserOrderSubscriptions[orderId].Add(connectionId);
        }

        await Groups.AddToGroupAsync(connectionId, orderId);
    }

    public async Task UnsubscribeFromOrder(string orderId)
    {
        var connectionId = Context.ConnectionId;

        lock (UserOrderSubscriptions)
        {
            if (UserOrderSubscriptions.ContainsKey(orderId))
            {
                UserOrderSubscriptions[orderId].Remove(connectionId);

                if (UserOrderSubscriptions[orderId].Count == 0)
                    UserOrderSubscriptions.Remove(orderId);
            }
        }

        await Groups.RemoveFromGroupAsync(connectionId, orderId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        lock (UserOrderSubscriptions)
        {
            foreach (var subscriptions in UserOrderSubscriptions.Values)
            {
                subscriptions.Remove(connectionId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}