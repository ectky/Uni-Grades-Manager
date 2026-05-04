using Microsoft.AspNetCore.Mvc;

namespace UserService.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> GetUsers()
        {
            return ["User 1", "User 2"];
        }
    }
}
