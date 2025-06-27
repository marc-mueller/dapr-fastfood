using FrontendSelfServicePos.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FrontendSelfServicePos.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            return Ok(new List<ProductDto>
            {
                new ProductDto {Id = Guid.NewGuid(), Title = "Cheeseburger", Price = 6.99m, Category = ProductCategory.Burgers, ProductDescription = "Classic cheeseburger with beef patty, cheese, and lettuce.", ImageUrl = new Uri("/images/cheeseburger.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Veggie Burger", Price = 5.49m, Category = ProductCategory.Burgers, ProductDescription = "Delicious veggie burger with lettuce and tomato.", ImageUrl = new Uri("/images/veggie_burger.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Chicken Burger", Price = 6.49m, Category = ProductCategory.Burgers, ProductDescription = "Crispy chicken burger with a special sauce.", ImageUrl = new Uri("/images/chicken_burger.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Fish Burger", Price = 6.99m, Category = ProductCategory.Burgers, ProductDescription = "Tasty fish burger with tartar sauce.", ImageUrl = new Uri("/images/fish_burger.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Double Burger", Price = 8.99m, Category = ProductCategory.Burgers, ProductDescription = "Double beef patty burger with cheese and lettuce.", ImageUrl = new Uri("/images/double_burger.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Classic Fries", Price = 2.99m, Category = ProductCategory.Fries, ProductDescription = "Crispy classic fries.", ImageUrl = new Uri("/images/classic_fries.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Curly Fries", Price = 3.49m, Category = ProductCategory.Fries, ProductDescription = "Seasoned curly fries.", ImageUrl = new Uri("/images/curly_fries.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Waffle Fries", Price = 3.79m, Category = ProductCategory.Fries, ProductDescription = "Waffle-shaped fries.", ImageUrl = new Uri("/images/waffle_fries.png")},
                new ProductDto {Id = Guid.NewGuid(), Title = "Sweet Potato Fries", Price = 3.99m, Category = ProductCategory.Fries, ProductDescription = "Sweet and crispy sweet potato fries.", ImageUrl = new Uri("/images/sweet_potato_fries.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Cheese Fries", Price = 4.49m, Category = ProductCategory.Fries, ProductDescription = "Fries topped with melted cheese.", ImageUrl = new Uri("/images/cheese_fries.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Cola", Price = 1.99m, Category = ProductCategory.Drinks, ProductDescription = "Refreshing cola.", ImageUrl = new Uri("/images/cola.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Lemonade", Price = 2.49m, Category = ProductCategory.Drinks, ProductDescription = "Freshly squeezed lemonade.", ImageUrl = new Uri("/images/lemonade.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Iced Tea", Price = 2.29m, Category = ProductCategory.Drinks, ProductDescription = "Chilled iced tea.", ImageUrl = new Uri("/images/iced_tea.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Orange Juice", Price = 2.99m, Category = ProductCategory.Drinks, ProductDescription = "Pure orange juice.", ImageUrl = new Uri("/images/orange_juice.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Water", Price = 1.49m, Category = ProductCategory.Drinks, ProductDescription = "Bottled water.", ImageUrl = new Uri("/images/water.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Ice Cream Sundae", Price = 3.99m, Category = ProductCategory.Desserts, ProductDescription = "Vanilla ice cream with chocolate sauce.", ImageUrl = new Uri("/images/ice_cream_sundae.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Apple Pie", Price = 2.99m, Category = ProductCategory.Desserts, ProductDescription = "Warm apple pie.", ImageUrl = new Uri("/images/apple_pie.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Chocolate Cake", Price = 4.49m, Category = ProductCategory.Desserts, ProductDescription = "Rich chocolate cake.", ImageUrl = new Uri("/images/chocolate_cake.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Brownie", Price = 2.49m, Category = ProductCategory.Desserts, ProductDescription = "Fudgy chocolate brownie.", ImageUrl = new Uri("/images/brownie.png", UriKind.Relative)},
                new ProductDto {Id = Guid.NewGuid(), Title = "Milkshake", Price = 3.79m, Category = ProductCategory.Desserts, ProductDescription = "Creamy vanilla milkshake.", ImageUrl = new Uri("/images/milkshake.png", UriKind.Relative)}
            });
        }
        catch(Exception ex)
        {
            LogFailedToRetrieveProducts(ex);
            return StatusCode(500, "Failed to retrieve products.");
        }
    }
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to retrieve products.")]
    private partial void LogFailedToRetrieveProducts(Exception ex);
}