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
    public OrderWorkflowController(IOrderProcessingServiceWorkflow orderProcessingService, IOrderServiceObservability observability, ILogger<OrderWorkflowController> logger) : base(orderProcessingService, observability, logger)
    {
       
    }
}