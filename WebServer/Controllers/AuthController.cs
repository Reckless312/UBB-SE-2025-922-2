using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using IAuthenticationService = DataAccess.Service.Authentication.Interfaces.IAuthenticationService;
using DataAccess.AuthProviders.Github;
using System.Text.Json;
using DataAccess.Model.AdminDashboard;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using DataAccess.Service.Authentication;

namespace WebServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IGitHubOAuthHelper gitHubOAuthHelper;
        private readonly IUserService userService;

        public AuthController(IAuthenticationService authenticationService, IGitHubOAuthHelper gitHubOAuthHelper, IUserService userService)
        {
            this.authenticationService = authenticationService;
            this.gitHubOAuthHelper = gitHubOAuthHelper;
            this.userService = userService;
        }

        public IActionResult MainWindow()
        {
            return View();
        }

        public IActionResult TwoFactorAuthSetup()
        {
            return View();
        }

        public IActionResult TwoFactorAuthCheck()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ManualLogin(string username, string password)
        {
            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Username and password are required!";
                return View("MainWindow");
            }
            AuthenticationResponse? authResponse = await this.authenticationService.AuthWithUserPass(username, password);
            if(!authResponse.AuthenticationSuccessful)
            {
                ViewBag.ErrorMessage = "Invalid username or password";
                return View("MainWindow");
            }
            User? user = await this.authenticationService.GetUser(authResponse.SessionId);
            if (string.IsNullOrEmpty(user.TwoFASecret) || !IsValidBase32(user.TwoFASecret))
            {
                System.Diagnostics.Debug.WriteLine("GitHubLogin: Generating new TwoFASecret for existing user");
                var key = KeyGeneration.GenerateRandomKey(20);
                user.TwoFASecret = Base32Encoding.ToString(key);
                await this.userService.UpdateUser(user);
            }
            string qrCode = GenerateQRCode(user.Username, user.TwoFASecret);
            ViewBag.QRCode = $"data:image/png;base64, {qrCode}";
            System.Diagnostics.Debug.WriteLine((string)ViewBag.QRCode);
            ViewBag.Username = user.Username;
            return View("TwoFactorAuthSetup");
        }

        public async Task<IActionResult> GitHubLogin()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GitHubLogin: Starting GitHub authentication");
                var authResponse = this.gitHubOAuthHelper.AuthenticateAsync().Result;
                if(!authResponse.AuthenticationSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine("GitHub authentication failed");
                    ViewBag.ErrorMessage = "GitHub authentication failed";
                    return RedirectToAction("MainWindow");
                }
                System.Diagnostics.Debug.WriteLine("GitHubLogin: Authentication successful");
                var userInfo = await FetchGitHubUserInfo(authResponse.OAuthToken);
                System.Diagnostics.Debug.WriteLine($"GitHubLogin: Fetched user info - Login: {userInfo.Login}, Name: {userInfo.Name}, Email: {userInfo.Email}");
                var existingUser = await this.userService.GetUserByUsername(userInfo.Login);
                User user;
                if (existingUser == null)
                {
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        Username = userInfo.Login,
                        FullName = userInfo.Name,
                        EmailAddress = userInfo.Email,
                        AssignedRole = RoleType.User,
                        PasswordHash = string.Empty,
                        TwoFASecret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20)),
                        NumberOfDeletedReviews = 0,
                        HasSubmittedAppeal = false
                    };
                    await this.userService.CreateUser(user);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("GitHubLogin: User exists, updating if necessary");
                    user = existingUser;

                    if (string.IsNullOrEmpty(user.TwoFASecret) || !IsValidBase32(user.TwoFASecret))
                    {
                        System.Diagnostics.Debug.WriteLine("GitHubLogin: Generating new TwoFASecret for existing user");
                        var key = KeyGeneration.GenerateRandomKey(20);
                        user.TwoFASecret = Base32Encoding.ToString(key);
                        await this.userService.UpdateUser(user);
                    }
                }
               
                string qrCode = GenerateQRCode(user.Username, user.TwoFASecret);
                ViewBag.QRCode = $"data:image/png;base64, {qrCode}";
                ViewBag.Username = user.Username;
                System.Diagnostics.Debug.WriteLine($"GitHubLogin: Redirecting to TwoFactorAuthSetup for user {user.Username}");
                return View("TwoFactorAuthSetup");
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = $"GitHub authentication failed: {ex.Message}";
                return RedirectToAction("MainWindow");
            }
        }

        [HttpPost]
        public async Task<IActionResult> VerifySetupCode(string[] digit, string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                System.Diagnostics.Debug.WriteLine("Username missing");
                ViewBag.ErrorMessage = "Username is missing.";
                return View("MainWindow");
            }

            string enteredCode = string.Join("", digit);
            User user;
            try
            {
                user = await this.userService.GetUserByUsername(username);
            }
            catch (ArgumentException)
            {
                ViewBag.ErrorMessage = "User not found";
                return View("MainWindow");
            }

            if (string.IsNullOrEmpty(user.TwoFASecret))
            {
                ViewBag.ErrorMessage = "Two-factor authentication is not set up for this user.";
                ViewBag.QRCode = $"data:image/png;base64, {GenerateQRCode(user.Username, user.TwoFASecret)}";
                ViewBag.Username = user.Username;
                return View("TwoFactorAuthSetup");
            }
            bool isCodeValid = VerifyTwoFactorCode(user.TwoFASecret, enteredCode);
            if (!isCodeValid)
            {
                ViewBag.ErrorMessage = "Invalid 2FA code";
                ViewBag.QRCode = $"data:image/png;base64, {GenerateQRCode(user.Username, user.TwoFASecret)}";
                ViewBag.Username = user.Username;
                return View("TwoFactorAuthSetup");
            }
            return RedirectToAction("UserPage", "User");
        }

        private string GenerateQRCode(string username, string twoFASecret)
        {
            string issuer = "DrinkDb";
            string totpUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(username)}?secret={twoFASecret}&issuer={Uri.EscapeDataString(issuer)}";

            using QRCodeGenerator? qrGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode? base64QRCode = new Base64QRCode(qrCodeData);
            return base64QRCode.GetGraphic(20);
        }

        private async Task<(string Login, string Name, string Email)> FetchGitHubUserInfo(string accessToken)
        {
            using HttpClient? client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add("User-Agent", "DrinkDb");

            var userResponse = await client.GetAsync("https://api.github.com/user");
            userResponse.EnsureSuccessStatusCode();
            var userResponseBody = await userResponse.Content.ReadAsStringAsync();
            var user = JsonDocument.Parse(userResponseBody);

            var login = user.RootElement.GetProperty("login").GetString() ?? string.Empty;
            var name = user.RootElement.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() ?? string.Empty : string.Empty;

            var emailResponse = await client.GetAsync("https://api.github.com/user/emails");
            emailResponse.EnsureSuccessStatusCode();
            var emailResponseBody = await emailResponse.Content.ReadAsStringAsync();
            var emails = JsonDocument.Parse(emailResponseBody);

            var email = string.Empty;
            foreach (var emailElement in emails.RootElement.EnumerateArray())
            {
                if (emailElement.GetProperty("primary").GetBoolean())
                {
                    email = emailElement.GetProperty("email").GetString() ?? string.Empty;
                    break;
                }
            }

            return (login, name, email);
        }

        private bool VerifyTwoFactorCode(string twoFASecret, string enteredCode)
        {
            var totp = new OtpNet.Totp(Base32Encoding.ToBytes(twoFASecret));
            return totp.VerifyTotp(enteredCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }

        private static bool IsValidBase32(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            foreach(char c in input.ToUpperInvariant())
            {
                if (!(c >= 'A' && c <= 'Z') && !(c >= '2' && c <= '7'))
                    return false;
            }
            return true;
        }
    }
}