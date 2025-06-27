using FinanceService.Observability;
using Microsoft.AspNetCore.Mvc;
using OrderPlacement.Services;
using OrderService.Common.Dtos;
using OrderService.Models.Helpers;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderActorController : OrderController
{
    public OrderActorController(IOrderProcessingServiceActor orderProcessingService, IOrderServiceObservability observability,  ILogger<OrderActorController> logger) : base(orderProcessingService, observability, logger)
    {
       
    }
}