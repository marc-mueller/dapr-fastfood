namespace FrontendSelfServicePos.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public Decimal Price { get; set; }
    public Uri? ImageUrl { get; set; }
}