using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> GetUsers()
        {
            return ["User 1", "User 2"];
        }
    }
}
