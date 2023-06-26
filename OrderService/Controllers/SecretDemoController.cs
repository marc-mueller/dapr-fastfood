using Dapr.Client;
using FastFood.Common;
using Microsoft.AspNetCore.Mvc;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecretDemoController : ControllerBase
{
    private readonly DaprClient _daprClient;

    public SecretDemoController(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }
    
    [HttpGet("{key}")]
    public async Task<ActionResult<string>> GetOrder(string key)
    {
        var secrets = await _daprClient.GetSecretAsync(FastFoodConstants.SecretStore, key);
        return Ok(secrets.Values.FirstOrDefault());
    }
}