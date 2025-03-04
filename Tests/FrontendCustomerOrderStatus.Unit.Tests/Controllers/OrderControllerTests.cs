using Dapr.Client;
using FastFood.Common;
using FrontendCustomerOrderStatus.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Common.Dtos;
using System.Net;
using Xunit;

namespace FrontendCustomerOrderStatus.Unit.Tests.Controllers;

public class OrderControllerTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly Mock<ILogger<OrderController>> _loggerMock;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _loggerMock = new Mock<ILogger<OrderController>>();
        _controller = new OrderController(_daprClientMock.Object, _loggerMock.Object);
    }

    
}