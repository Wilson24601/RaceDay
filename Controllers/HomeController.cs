using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("RaceDay API is running. Navigate to /swagger to test endpoints.");
        }
    }
}
