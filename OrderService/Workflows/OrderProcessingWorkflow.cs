using Dapr.Workflow;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;

namespace OrderPlacement.Workflows;

public class OrderProcessingWorkflow : Workflow<Guid, Order>
{
    public override async Task<Order> RunAsync(WorkflowContext context, Guid orderId)
    {
        Order order = null;
        try
        {
            // Create order
            order = await context.CallActivityAsync<Order>(nameof(CreateOrderActivity), orderId);

            // Configure the order. The customer is assigned with invoice and delivery address. The customer and delivery address are optional for in house orders.
            // Items can be added or removed to the order until the order is confirmed.
            // Once the order is confirmed, the order is ready for payment.
            while (order.State == OrderState.Creating)
            {
                using var cts = new CancellationTokenSource();
                
                var assignCustomerEvent = context.WaitForExternalEventAsync<AssignCustomerEvent>(AssignCustomerEvent.Name, cts.Token);
                var assignInvoiceAddressEvent = context.WaitForExternalEventAsync<AssignInvoiceAddressEvent>(AssignInvoiceAddressEvent.Name, cts.Token);
                var assignDeliveryAddressEvent = context.WaitForExternalEventAsync<AssignDeliveryAddressEvent>(AssignDeliveryAddressEvent.Name, cts.Token);
                var addItemEvent = context.WaitForExternalEventAsync<AddItemEvent>(AddItemEvent.Name, cts.Token);
                var removeItemEvent = context.WaitForExternalEventAsync<RemoveItemEvent>(RemoveItemEvent.Name, cts.Token);
                var confirmOrderEvent = context.WaitForExternalEventAsync<ConfirmOrderEvent>(ConfirmOrderEvent.Name, cts.Token);
                var receivedEvent = await Task.WhenAny(assignCustomerEvent, assignInvoiceAddressEvent, assignDeliveryAddressEvent, addItemEvent, removeItemEvent, confirmOrderEvent);
                await cts.CancelAsync();
                if (receivedEvent == assignCustomerEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AssignCustomerActivity), await assignCustomerEvent);
                }
                else if (receivedEvent == assignInvoiceAddressEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AssignInvoiceAddressActivity), await assignInvoiceAddressEvent);
                }
                else if (receivedEvent == assignDeliveryAddressEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AssignDeliveryAddressActivity), await assignDeliveryAddressEvent);
                }
                else if (receivedEvent == addItemEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AddItemActivity), await addItemEvent);
                }
                else if (receivedEvent == removeItemEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(RemoveItemActivity), await removeItemEvent);
                }
                else if (receivedEvent == confirmOrderEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(ConfirmOrderActivity), await confirmOrderEvent);
                }
                else
                {
                    context.SetCustomStatus("No event received");
                }
            } 
    
            // Wait for the payment confirmation.
            var confirmPaymentEvent = await context.WaitForExternalEventAsync<ConfirmPaymentEvent>(ConfirmPaymentEvent.Name);
            order = await context.CallActivityAsync<Order>(nameof(ConfirmPaymentActivity), confirmPaymentEvent);

            // Wait for the order processing to start.
            var startProcessingEvent = await context.WaitForExternalEventAsync<StartProcessingEvent>(StartProcessingEvent.Name);
            order = await context.CallActivityAsync<Order>(nameof(StartProcessingActivity), startProcessingEvent);
            
            // Wait for all of the items to be prepared. The individual items can be perpared in parallel.
            while (order.State == OrderState.Processing)
            {
                var finishedItemEvent = await context.WaitForExternalEventAsync<ItemFinishedEvent>(ItemFinishedEvent.Name);
                order = await context.CallActivityAsync<Order>(nameof(ItemFinishedActivity), finishedItemEvent);
            }

            var orderServedEvent = await context.WaitForExternalEventAsync<OrderServedEvent>(OrderServedEvent.Name);
            order = await context.CallActivityAsync<Order>(nameof(OrderServedActivity), orderServedEvent);
            
            return order;
        }
        catch (Exception e)
        {
            context.SetCustomStatus($"Something went wrong: {e.Message}");
            throw;
        }
    }
}