using Dapr.Client;
using FastFood.Common;
using FrontendKitchenMonitor.Controllers;
using KitchenService.Common.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace FrontendKitchenMonitor.Unit.Tests.Controllers;

public class KitchenWorkControllerTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly Mock<ILogger<KitchenWorkController>> _loggerMock;
    private readonly KitchenWorkController _controller;

    public KitchenWorkControllerTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _loggerMock = new Mock<ILogger<KitchenWorkController>>();
        _controller = new KitchenWorkController(_daprClientMock.Object, _loggerMock.Object);
    }

    
}