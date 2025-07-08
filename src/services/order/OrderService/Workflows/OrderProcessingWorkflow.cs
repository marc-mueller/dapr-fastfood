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

            while (order.State == OrderState.Creating)
            {
                var createOrderWrapperEvent = await context.WaitForExternalEventAsync<CreateOrderWrapperEvent>(CreateOrderWrapperEvent.Name);
                if (createOrderWrapperEvent.SubEventType == CreateOrderWrapperEvent.CreateOrderSubEventType.AssignCustomerEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AssignCustomerActivity), createOrderWrapperEvent.AssignCustomerEvent);
                }
                else if (createOrderWrapperEvent.SubEventType == CreateOrderWrapperEvent.CreateOrderSubEventType.AssignInvoiceAddressEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AssignInvoiceAddressActivity), createOrderWrapperEvent.AssignInvoiceAddressEvent);
                }
                else if (createOrderWrapperEvent.SubEventType == CreateOrderWrapperEvent.CreateOrderSubEventType.AssignDeliveryAddressEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AssignDeliveryAddressActivity), createOrderWrapperEvent.AssignDeliveryAddressEvent);
                }
                else if (createOrderWrapperEvent.SubEventType == CreateOrderWrapperEvent.CreateOrderSubEventType.AddItemEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(AddItemActivity), createOrderWrapperEvent.AddItemEvent);
                }
                else if (createOrderWrapperEvent.SubEventType == CreateOrderWrapperEvent.CreateOrderSubEventType.RemoveItemEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(RemoveItemActivity), createOrderWrapperEvent.RemoveItemEvent);
                }
                else if (createOrderWrapperEvent.SubEventType == CreateOrderWrapperEvent.CreateOrderSubEventType.ConfirmOrderEvent)
                {
                    order = await context.CallActivityAsync<Order>(nameof(ConfirmOrderActivity), createOrderWrapperEvent.ConfirmOrderEvent);
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
            return order ?? new Order(){ Id = orderId};
        }
    }
}