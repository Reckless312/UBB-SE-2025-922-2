using Microsoft.AspNetCore.Mvc;
using DataAccess.AuthProviders;
using DataAccess.Model.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class BasicAuthenticationProviderController : ControllerBase
    {
        private readonly IBasicAuthenticationProvider basicAuthProvider;
        private readonly IUserService userService;

        public BasicAuthenticationProviderController(IBasicAuthenticationProvider basicAuthProvider, IUserService userService)
        {
            this.basicAuthProvider = basicAuthProvider;
            this.userService = userService;
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<bool>> Authenticate([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password must be provided.");
            }
            bool isAuthenticated = await this.basicAuthProvider.AuthenticateAsync(request.Username, request.Password);

            if (!isAuthenticated)
            {
                return NotFound($"User {request.Username} not found.");
            }
            return Ok(isAuthenticated);
        }

        [HttpGet("user/{username}")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            User? user = await this.userService.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound($"User {username} not found.");
            }

            return Ok(user);
        }
    }
}
