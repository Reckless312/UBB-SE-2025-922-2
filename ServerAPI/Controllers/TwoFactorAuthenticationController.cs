using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Components;
using DataAccess.Service.Authentication.Interfaces;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using OtpNet;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    public class TwoFactorAuthenticationController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IKeyGeneration keyGeneration;

        public TwoFactorAuthenticationController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
            this.keyGeneration = new OtpKeyGeneration();
        }

        [HttpPost("setup")]
        public async Task<IActionResult> Setup2FA([FromBody] TwoFASetupRequest request)
        {
            User? user = await this.userRepository.GetUserById(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            byte[] twoFactorSecret;
            string uniformResourceIdentifier = string.Empty;

            if (request.IsFirstTimeSetup)
            {
                twoFactorSecret = this.keyGeneration.GenerateRandomKey(42);
                user.TwoFASecret = Convert.ToBase64String(twoFactorSecret);
                await this.userRepository.UpdateUser(user);
                uniformResourceIdentifier = new OtpUri(OtpType.Totp, twoFactorSecret, user.Username, "DrinkDB").ToString();
            }
            else
            {
                twoFactorSecret = Convert.FromBase64String(user.TwoFASecret ?? string.Empty);
            }

            return Ok(new
            {
                User = user,
                UniformResourceIdentifier = uniformResourceIdentifier,
                TwoFactorSecret = Convert.ToBase64String(twoFactorSecret)
            });
        }

        public class TwoFASetupRequest
        {
            public Guid UserId { get; set; }
            public bool IsFirstTimeSetup { get; set; }
        }
    }
}