using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        [HttpGet("can-user-login")]
        public bool CanUserLogin()
        {
            return true;
        }
    }
}
