namespace FinanceService.Common.Dtos;

public class RevenueReportDto
{
    public DateTimeOffset FiscalPeriodStart { get; set; }
    public DateTimeOffset FiscalPeriodEnd { get; set; }
    public string Location { get; set; } = "";
    public decimal Revenue { get; set; }
    public string Currency { get; set; } = "CHF";
}