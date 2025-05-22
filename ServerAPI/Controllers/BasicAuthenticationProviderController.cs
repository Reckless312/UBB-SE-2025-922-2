using Microsoft.AspNetCore.Mvc;
using DataAccess.AuthProviders;
using DataAccess.Model.Authentication;
using IRepository;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class BasicAuthenticationProviderController : ControllerBase
    {
        private readonly IBasicAuthenticationProvider basicAuthProvider;
        private readonly IUserRepository userRepository;

        public BasicAuthenticationProviderController(
            IBasicAuthenticationProvider basicAuthProvider,
            IUserRepository userRepository)
        {
            this.basicAuthProvider = basicAuthProvider;
            this.userRepository = userRepository;
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<bool>> Authenticate([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password must be provided.");

            try
            {
                bool isAuthenticated = await this.basicAuthProvider.AuthenticateAsync(request.Username, request.Password);
                return Ok(isAuthenticated);
            }
            catch (UserNotFoundException)
            {
                return NotFound($"User {request.Username} not found.");
            }
        }

        [HttpGet("user/{username}")]
        public async Task<ActionResult<User>> GetUserByUsername(string username)
        {
            try
            {
                User? user = await this.userRepository.GetUserByUsername(username);
                if (user == null)
                    return NotFound($"User {username} not found.");
                
                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }
    }
}
