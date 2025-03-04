using FinanceService.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RevenueReportController: ControllerBase
{
    private readonly ILogger<OrderFinanceController> _logger;

    public RevenueReportController(ILogger<OrderFinanceController> logger)
    {
            _logger = logger;
    }
    
    [HttpGet("ByYear/{year:int=2025}")]
    public ActionResult<List<RevenueReportDto>> RevenueByYear(int year = 2025)
    {
        _logger.LogInformation("Revenue report requested for year: {year}", year);
    
        // Define major Swiss cities
        var cities = new[] { "Zurich", "Geneva", "Basel", "Lausanne", "Bern" };
        var random = new Random();
        
        // Determine the period for the reports
        var startDate = new DateTime(year, 1, 1);
        var endDate = year < DateTime.UtcNow.Year ? new DateTime(year, 12, 31) : DateTime.UtcNow;
    
        // Generate demo RevenueReportDto objects
        var reports = Enumerable.Range(1, 5).Select(i => new RevenueReportDto
        {
            FiscalPeriodStart = startDate,
            FiscalPeriodEnd = endDate,
            Location = cities[random.Next(cities.Length)],
            Revenue = random.Next(50000, 1000001),
            Currency = "CHF"
        }).ToList();
    
        return Ok(reports);
    }
    
}