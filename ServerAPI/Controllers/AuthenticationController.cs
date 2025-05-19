using Microsoft.AspNetCore.Mvc;
using DataAccess.Service.Authentication;
using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Interfaces;
using IRepository;
using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.Github;
using DataAccess.AuthProviders.LinkedIn;
using DataAccess.AuthProviders;
using DataAccess.OAuthProviders;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService authenticationService;

        public AuthenticationController(
            ISessionRepository sessionRepository,
            IUserRepository userRepository,
            ILinkedInLocalOAuthServer linkedinLocalOAuth,
            IGitHubLocalOAuthServer githubLocalOAuth,
            IFacebookLocalOAuthServer facebookLocalOAuth,
            IBasicAuthenticationProvider basicAuthProvider)
        {
            authenticationService = new AuthenticationService(
                sessionRepository,
                userRepository,
                linkedinLocalOAuth,
                githubLocalOAuth,
                facebookLocalOAuth,
                basicAuthProvider
            );
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> LoginWithCredentials([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password must be provided.");

            var response = await authenticationService.AuthWithUserPass(request.Username, request.Password);
            if (!response.AuthenticationSuccessful)
                return Unauthorized();

            return Ok(response);
        }

        [HttpPost("oauth")]
        public async Task<ActionResult<AuthenticationResponse>> LoginWithOAuth([FromQuery] OAuthService service)
        {
            object helper = service switch
            {
                OAuthService.GitHub => HttpContext.RequestServices.GetService(typeof(IGitHubOAuthHelper)),
                OAuthService.Facebook => HttpContext.RequestServices.GetService(typeof(IFacebookOAuthHelper)),
                OAuthService.LinkedIn => HttpContext.RequestServices.GetService(typeof(ILinkedInOAuthHelper)),
                _ => null
            };

            if (helper == null)
                return BadRequest("OAuth service not supported or helper not found.");

            var response = await authenticationService.AuthWithOAuth(service, helper);
            if (!response.AuthenticationSuccessful)
                return Unauthorized();

            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            authenticationService.Logout();
            return Ok("Logged out successfully.");
        }

        [HttpGet("user")]
        public async Task<ActionResult<User>> GetCurrentUser([FromQuery] Guid sessionId)
        {
            try
            {
                var user = await authenticationService.GetUser(sessionId);
                return Ok(user);
            }
            catch (UserNotFoundException)
            {
                return NotFound("User not found for the given session.");
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
