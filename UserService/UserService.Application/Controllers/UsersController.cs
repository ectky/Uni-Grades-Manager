using Microsoft.AspNetCore.Mvc;

namespace UserService.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {

        [HttpGet(Name = "GetWeatherForecast")]
        public string Get()
        {
            return "";
        }
    }
}
