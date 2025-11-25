using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using FastFood.FeatureManagement.Common.Constants;
using FastFood.FeatureManagement.Common.Services;
using OrderPlacement.Services;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

public partial class ConfirmPaymentActivity : WorkflowActivity<ConfirmPaymentEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<ConfirmPaymentActivity> _logger;
    private readonly DaprClient _daprClient;
    private readonly IOrderPricingService _pricingService;
    private readonly IObservableFeatureManager _featureManager;

    public ConfirmPaymentActivity(
        IOrderStorage orderStorage, 
        DaprClient daprClient, 
        ILogger<ConfirmPaymentActivity> logger,
        IOrderPricingService pricingService,
        IObservableFeatureManager featureManager)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
        _pricingService = pricingService;
        _featureManager = featureManager;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, ConfirmPaymentEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null)
        {
            if (order.State == OrderState.Confirmed)
            {
                order.State = OrderState.Paid;
                await _orderStorage.UpdateOrder(order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, order.ToDto());
                
                // Calculate pricing breakdown with service fees
                var pricing = await _pricingService.CalculateOrderPricing(order);
                
                // Check for loyalty discount
                decimal? discount = null;
                var loyaltyEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.LoyaltyProgram);
                if (loyaltyEnabled && !string.IsNullOrEmpty(order.Customer?.LoyaltyNumber))
                {
                    discount = Math.Round(pricing.Subtotal * 0.10m, 2);
                }
                
                // Send to FinanceService with pricing breakdown
                await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/newOrder", order.ToFinanceDto(pricing.ServiceFee, discount));
                LogPaymentConfirmed(context.InstanceId, order.Id);
            }
            else
            {
                // order already paid, idempotent operation
            }
        }
        else
        {
            LogPaymentConfirmedFailed(context.InstanceId, input.OrderId);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Confirmed payment for order {orderId}")]
    private partial void LogPaymentConfirmed(string instanceId, Guid orderId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to confirm payment for order {orderId}")]
    private partial void LogPaymentConfirmedFailed(string instanceId, Guid orderId);
}