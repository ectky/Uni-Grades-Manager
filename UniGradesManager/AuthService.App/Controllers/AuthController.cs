using Microsoft.AspNetCore.Mvc;
using RestSharp;
using RestSharp.Authenticators;

namespace AuthService.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<string>> ReceiveUsers()
        {
            var options = new RestClientOptions("https://localhost:7159");
            var client = new RestClient(options);

            var request = new RestRequest("Users");
            request.AddHeader("Authorization", $"Bearer ");

            var timeline = await client.GetAsync<IEnumerable<string>>(request);

            return timeline;
        }
    }
}
