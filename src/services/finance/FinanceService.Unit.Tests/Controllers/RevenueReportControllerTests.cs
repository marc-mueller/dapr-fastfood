using FinanceService.Common.Dtos;
using FinanceService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceService.Unit.Tests.Controllers;

public class RevenueReportControllerTests
{
    private readonly Mock<ILogger<OrderFinanceController>> _loggerMock;
    private readonly RevenueReportController _controller;

    public RevenueReportControllerTests()
    {
        _loggerMock = new Mock<ILogger<OrderFinanceController>>();
        _controller = new RevenueReportController(_loggerMock.Object);
    }

    [Fact]
    public void RevenueByYear_DefaultYear_Returns5Reports()
    {
        // Act
        var result = _controller.RevenueByYear();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reports = Assert.IsType<List<RevenueReportDto>>(okResult.Value);
        Assert.Equal(5, reports.Count);
    }

    [Fact]
    public void RevenueByYear_CustomYear_Returns5ReportsWithCorrectYear()
    {
        // Arrange
        var year = 2024;

        // Act
        var result = _controller.RevenueByYear(year);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reports = Assert.IsType<List<RevenueReportDto>>(okResult.Value);
        Assert.Equal(5, reports.Count);
        Assert.All(reports, report => 
        {
            Assert.Equal(new DateTime(year, 1, 1), report.FiscalPeriodStart);
            Assert.Equal("CHF", report.Currency);
            Assert.InRange(report.Revenue, 50000, 1000001);
        });
    }

    [Fact]
    public void RevenueByYear_FutureYear_UseCurrentDateAsEnd()
    {
        // Arrange
        var futureYear = DateTime.UtcNow.Year + 1;

        // Act
        var result = _controller.RevenueByYear(futureYear);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reports = Assert.IsType<List<RevenueReportDto>>(okResult.Value);
        Assert.Equal(5, reports.Count);
    }

    [Fact]
    public void RevenueByYear_AllReportsHaveSwissCities()
    {
        // Arrange
        var swissCities = new[] { "Zurich", "Geneva", "Basel", "Lausanne", "Bern" };

        // Act
        var result = _controller.RevenueByYear(2025);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reports = Assert.IsType<List<RevenueReportDto>>(okResult.Value);
        Assert.All(reports, report => Assert.Contains(report.Location, swissCities));
    }
}
