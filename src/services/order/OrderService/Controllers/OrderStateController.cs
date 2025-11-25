using FastFood.FeatureManagement.Common.Services;
using FinanceService.Observability;
using Microsoft.AspNetCore.Mvc;
using OrderPlacement.Services;
using OrderService.Common.Dtos;
using OrderService.Models.Helpers;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderStateController : OrderController
{
    public OrderStateController(
        IOrderProcessingServiceState orderProcessingService, 
        IOrderServiceObservability observability, 
        IObservableFeatureManager featureManager,
        IOrderPricingService pricingService,
        ILogger<OrderStateController> logger) 
        : base(orderProcessingService, observability, featureManager, pricingService, logger)
    {
       
    }
}