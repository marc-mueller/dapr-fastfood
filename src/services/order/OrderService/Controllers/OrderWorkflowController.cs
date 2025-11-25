using FastFood.FeatureManagement.Common.Services;
using FinanceService.Observability;
using Microsoft.AspNetCore.Mvc;
using OrderPlacement.Services;
using OrderService.Common.Dtos;
using OrderService.Models.Helpers;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderWorkflowController : OrderController
{
    public OrderWorkflowController(
        IOrderProcessingServiceWorkflow orderProcessingService, 
        IOrderServiceObservability observability, 
        IObservableFeatureManager featureManager,
        IOrderPricingService pricingService,
        ILogger<OrderWorkflowController> logger) 
        : base(orderProcessingService, observability, featureManager, pricingService, logger)
    {
       
    }
}