using Microsoft.AspNetCore.Mvc;

namespace DemoDeck.Auth.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                service = "DemoDeck.Auth.Api",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}