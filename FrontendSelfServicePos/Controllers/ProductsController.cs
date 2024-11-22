using FrontendSelfServicePos.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FrontendSelfServicePos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            return Ok(new List<ProductDto>
            {
                new ProductDto { Id = Guid.NewGuid(), Title = "Burger", Price = 5.99m },
                new ProductDto { Id = Guid.NewGuid(), Title = "Fries", Price = 2.99m },
                new ProductDto { Id = Guid.NewGuid(), Title = "Soda", Price = 1.99m }
            });
        }
        catch
        {
            return StatusCode(500, "Failed to retrieve products.");
        }
    }
}