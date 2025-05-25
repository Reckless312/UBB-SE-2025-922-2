using System.Diagnostics;
using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.Github;
using DataAccess.AuthProviders.LinkedIn;
using DataAccess.AuthProviders.Twitter;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication;
using DataAccess.Service.Authentication.Interfaces;
using DrinkDb_Auth.AuthProviders.Google;
using DrinkDb_Auth.Service.Authentication.Components;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using static QRCoder.PayloadGenerator;

namespace WebServer.Controllers
{
    public class AuthController : Controller
    {
        private IAuthenticationService authenticationService;
        private ITwoFactorAuthenticationService twoFactorAuthenticationService;
        private IVerify twoFactorVerify = new Verify2FactorAuthenticationSecret();
        private IGitHubOAuthHelper gitHubOAuthHelper;
        private IFacebookOAuthHelper facebookOAuthHelper;
        private ILinkedInOAuthHelper linkedInOAuthHelper;
        private ITwitterOAuth2Provider twitterOAuth2Provider;
        private IGoogleOAuth2Provider googleOAuth2Provider;
        private IUserService userService;
        public const int KEY_LENGTH = 20;

        private const string GOOGLE_WEB_CLIENT_ID = "886406538781-ufhtu1f11p331ap2gbbj43mq70626jsb.apps.googleusercontent.com";
        private const string GOOGLE_WEB_CLIENT_SECRET = "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";
        private const string GOOGLE_URL = "http://127.0.0.1:5000/";

        public AuthController(IAuthenticationService authenticationService, ITwoFactorAuthenticationService twoFactorAuthenticationService, IUserService userService,
                IGitHubOAuthHelper gitHubOAuthHelper, ILinkedInOAuthHelper linkedInOAuthHelper, IFacebookOAuthHelper facebookOAuthHelper,
                ITwitterOAuth2Provider twitterOAuth2Provider, IGoogleOAuth2Provider googleOAuth2Provider)
        {
            this.authenticationService = authenticationService;
            this.twoFactorAuthenticationService = twoFactorAuthenticationService;
            this.userService = userService;
            this.gitHubOAuthHelper = gitHubOAuthHelper;
            this.linkedInOAuthHelper = linkedInOAuthHelper;
            this.facebookOAuthHelper = facebookOAuthHelper;
            this.twitterOAuth2Provider = twitterOAuth2Provider;
            this.googleOAuth2Provider = googleOAuth2Provider;
        }

        public IActionResult MainWindow()
        {
            return View();
        }

        public IActionResult TwoFactorAuthSetup()
        {
            return View();
        }

        private async Task<IActionResult> AuthenticationComplete(AuthenticationResponse response)
        {
            if (response.AuthenticationSuccessful)
            {
                User? user = await this.authenticationService.GetUser(response.SessionId);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "Couldn't find user!";
                    return View("MainWindow");
                }

                bool firstTimeSetup = string.IsNullOrEmpty(user.TwoFASecret);

                (User? currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) = await this.Update2FAValues(user, firstTimeSetup);

                if (currentUser == null)
                {
                    ViewBag.ErrorMessage = "Couldn't find user!";
                    return View("MainWindow");
                }

                if (currentUser.TwoFASecret == null)
                {
                    ViewBag.ErrorMessage = "Two factor secret not set up!";
                    return View("MainWindow");
                }

                if (firstTimeSetup)
                {
                    ViewBag.ShowQRCode = true;
                    string qrCode = GenerateQRCode(currentUser.Username, currentUser.TwoFASecret, uniformResourceIdentifier);
                    ViewBag.QRCode = $"data:image/png;base64, {qrCode}";
                }
                else
                {
                    ViewBag.ShowQRCode = false;
                }

                AuthenticationService.SetCurrentSessionId(response.SessionId);
                AuthenticationService.SetCurrentUserId(currentUser.UserId);

                ViewBag.Username = currentUser.Username;
                return View("TwoFactorAuthSetup");
            }
            else
            {
                ViewBag.ErrorMessage = "Something went wrong on login, please try again";
                return View("MainWindow");
            }
        }

        private async Task<(User?, string, byte[])> Update2FAValues(User? user, bool firstTimeSetup)
        {
            if (user == null)
            {
                ViewBag.ErrorMessage = "Couldn't find user!";
                return (null, string.Empty, Array.Empty<byte>());
            }

            this.twoFactorAuthenticationService.UserId = user.UserId;
            this.twoFactorAuthenticationService.IsFirstTimeSetup = firstTimeSetup;

            return await this.twoFactorAuthenticationService.Get2FAValues();
        }

        [HttpPost]
        public async Task<IActionResult> ManualLogin(string username, string password)
        {
            AuthenticationResponse response = await this.authenticationService.AuthWithUserPass(username, password);

            return await this.AuthenticationComplete(response);
        }

        public async Task<IActionResult> GitHubLogin()
        {
            AuthenticationResponse response = await this.authenticationService.AuthWithOAuth(OAuthService.GitHub, this.gitHubOAuthHelper);

            return await this.AuthenticationComplete(response);
        }

        public async Task<IActionResult> FacebookLogin()
        {
            AuthenticationResponse response = await this.authenticationService.AuthWithOAuth(OAuthService.Facebook, this.facebookOAuthHelper);

            return await this.AuthenticationComplete(response);
        }

        public async Task<IActionResult> LinkedInLogin()
        {
            AuthenticationResponse response = await this.authenticationService.AuthWithOAuth(OAuthService.LinkedIn, this.linkedInOAuthHelper);

            return await this.AuthenticationComplete(response);
        }

        [HttpGet("twitter/login")]
        public IActionResult TwitterLogin()
        {
            string authorizationUrl = ((TwitterOAuth2Provider)this.twitterOAuth2Provider).GetAuthorizationUrl();

            TempData["TwitterCode"] = ((TwitterOAuth2Provider)this.twitterOAuth2Provider).codeVerifier;

            return Redirect(authorizationUrl);
        }

        [HttpGet("x-callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View("MainWindow");
            }

            string codeVerifier = TempData["TwitterCode"] as string ?? string.Empty;

            ((TwitterOAuth2Provider)this.twitterOAuth2Provider).codeVerifier = codeVerifier;

            string fullCallbackUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";

            string receivedAuthCode = this.twitterOAuth2Provider.ExtractQueryParameter(fullCallbackUrl, "code");

            AuthenticationResponse result = await this.twitterOAuth2Provider.ExchangeCodeForTokenAsync(receivedAuthCode);

            return await this.AuthenticationComplete(result);
        }

        [HttpGet("google/login")]
        public IActionResult GoogleLogin()
        {
            ((GoogleOAuth2Provider)this.googleOAuth2Provider).ClientId = AuthController.GOOGLE_WEB_CLIENT_ID;
            ((GoogleOAuth2Provider)this.googleOAuth2Provider).ClientSecret = AuthController.GOOGLE_WEB_CLIENT_SECRET;
            ((GoogleOAuth2Provider)this.googleOAuth2Provider).RedirectUniformResourceIdentifier = AuthController.GOOGLE_URL;

            string authorizationUrl = ((GoogleOAuth2Provider)this.googleOAuth2Provider).GetAuthorizationUrl();

            return Redirect(authorizationUrl);
        }

        [HttpGet("")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code))
            {
                return View("MainWindow");
            }

            ((GoogleOAuth2Provider)this.googleOAuth2Provider).ClientId = AuthController.GOOGLE_WEB_CLIENT_ID;
            ((GoogleOAuth2Provider)this.googleOAuth2Provider).ClientSecret = AuthController.GOOGLE_WEB_CLIENT_SECRET;
            ((GoogleOAuth2Provider)this.googleOAuth2Provider).RedirectUniformResourceIdentifier = AuthController.GOOGLE_URL;

            AuthenticationResponse result = await this.googleOAuth2Provider.ExchangeCodeForTokenAsync(code);

            return await this.AuthenticationComplete(result);
        }

        [HttpPost]
        public async Task<IActionResult> VerifySetupCode(string[] digit, string username)
        {
            string enteredCode = string.Join(string.Empty, digit);

            User? user = await this.userService.GetUserByUsername(username);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found!";
                return View("MainWindow");
            }

            bool firstTimeSetup = string.IsNullOrEmpty(user.TwoFASecret);

            (User? currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) = await this.Update2FAValues(user, firstTimeSetup);

            if (!this.twoFactorVerify.Verify2FAForSecret(twoFactorSecret, enteredCode))
            {
                ViewBag.ErrorMessage = "Invalid 2FA code. Please try again.";
                return View("TwoFactorAuthSetup");
            }
            return RedirectToAction("SuccessPage", "Success");
        }

        private string GenerateQRCode(string username, string twoFASecret, string totpUri)
        {
            using QRCodeGenerator? qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode? base64QRCode = new Base64QRCode(qrCodeData);
            return base64QRCode.GetGraphic(KEY_LENGTH);
        }

        public IActionResult Cancel2FA()
        {
            return RedirectToAction("MainWindow");
        }
    }
}