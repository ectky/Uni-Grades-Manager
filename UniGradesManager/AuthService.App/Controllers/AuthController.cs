using AuthService.App.Services.Contracts;
using AuthService.App.VMs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using RestSharp;
using RestSharp.Authenticators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _privateKey;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _privateKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Secrets:JWTSecretKey"]));
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IResult> Login([FromForm] LoginVm request)
        {
            string loggedUsername = User.FindFirst(ClaimTypes.Name)?.Value;
            if (loggedUsername != null)
            {
                return Results.Forbid();
            }

            if (!await this._userService.CanUserLoginAsync(request.Username, request.Password))
            {
                return Results.BadRequest();
            }

            var creds = new SigningCredentials(_privateKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.Username)
            };

            const int TokenLifetimeMinutes = 10;    
            var jwt = new JwtSecurityToken(
                issuer: "https://localhost:7131",
                audience: "https://localhost:7159",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes),
                signingCredentials: creds);

            return Results.Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(jwt),
                expires_in = TokenLifetimeMinutes * 60
            });
        }

        //[HttpPost]
        //public async Task<IActionResult> Register([FromForm] RegisterVM userCreateModel)
        //{
        //    string loggedUsername = User.FindFirst(ClaimTypes.Name)?.Value;

        //    if (loggedUsername != null)
        //    {
        //        return Forbid();
        //    }

        //    if (await this.usersService.GetByUsernameAsync(userCreateModel.Username) != default)
        //    {
        //        return BadRequest(Constants.UserAlreadyExists);
        //    }

        //    var hashedPassword = PasswordHasher.HashPassword(userCreateModel.Password);
        //    userCreateModel.Password = hashedPassword;

        //    var userDto = this.mapper.Map<UserDto>(userCreateModel);
        //    userDto.RoleId = (await rolesService.GetByNameIfExistsAsync(UserRole.User.ToString()))?.Id;
        //    await this.usersService.SaveAsync(userDto);
        //    await LoginUser(userDto.Username);

        //    return RedirectToAction(nameof(HomeController.Index), "Home");
        //}
        //[HttpGet]
        //public async Task<IActionResult> Logout()
        //{
        //    string loggedUsername = User.FindFirst(ClaimTypes.Name)?.Value;
        //    if (loggedUsername != null)
        //    {
        //        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    }
        //    return RedirectToAction(nameof(HomeController.Index), "Home");
        //}
    }

}