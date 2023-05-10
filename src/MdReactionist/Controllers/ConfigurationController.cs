using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MdReactionist.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly BotOptions _botOptions;

    public ConfigurationController(IOptions<BotOptions> botOptions)
    {
        _botOptions = botOptions.Value;
    }
    
    [HttpGet]
    public IActionResult GetConfiguration()
    {
        return Ok(_botOptions);
    }
}
