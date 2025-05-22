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
using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.LinkedIn;
using BCrypt.Net;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Azure.Core;

namespace WebServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationService authenticationService;
        private readonly IGitHubOAuthHelper gitHubOAuthHelper;
        private readonly IUserService userService;
        private readonly IFacebookOAuthHelper facebookOAuthHelper;
        private readonly ILinkedInOAuthHelper linkedInOAuthHelper;

        public AuthController(IAuthenticationService authenticationService, IGitHubOAuthHelper gitHubOAuthHelper, IUserService userService, IFacebookOAuthHelper facebookOAuthHelper, ILinkedInOAuthHelper linkedInOAuthHelper)
        {
            this.authenticationService = authenticationService;
            this.gitHubOAuthHelper = gitHubOAuthHelper;
            this.userService = userService;
            this.facebookOAuthHelper = facebookOAuthHelper;
            this.linkedInOAuthHelper = linkedInOAuthHelper;
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
            User user = null;
            bool isNewUser = false;
            try
            {
                user = await this.userService.GetUserByUsername(username);
            }
            catch
            {
                isNewUser = true;
                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password))), 
                    TwoFASecret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20)),
                    EmailAddress = string.Empty,
                    NumberOfDeletedReviews = 0,
                    HasSubmittedAppeal = false,
                    AssignedRole = RoleType.User,
                    FullName = string.Empty
                };
                await this.userService.CreateUser(user);
            }
            AuthenticationResponse? authResponse = await this.authenticationService.AuthWithUserPass(username, password);
            if(!authResponse.AuthenticationSuccessful)
            {
                ViewBag.ErrorMessage = "Invalid username or password";
                return View("MainWindow");
            }
            if (string.IsNullOrEmpty(user.TwoFASecret) || !IsValidBase32(user.TwoFASecret))
            {
                System.Diagnostics.Debug.WriteLine("GitHubLogin: Generating new TwoFASecret for existing user");
                var key = KeyGeneration.GenerateRandomKey(20);
                user.TwoFASecret = Base32Encoding.ToString(key);
                await this.userService.UpdateUser(user);
            }
            ViewBag.ShowQRCode = isNewUser;
            if (isNewUser)
            {
                string qrCode = GenerateQRCode(user.Username, user.TwoFASecret);
                ViewBag.QRCode = $"data:image/png;base64, {qrCode}";
            }
            ViewBag.Username = user.Username;
            return View("TwoFactorAuthSetup");
        }

        public async Task<IActionResult> GitHubLogin()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GitHubLogin: Starting GitHub authentication");
                AuthenticationResponse? authResponse = await this.gitHubOAuthHelper.AuthenticateAsync();
                if(!authResponse.AuthenticationSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine("GitHub authentication failed");
                    ViewBag.ErrorMessage = "GitHub authentication failed";
                    return View("MainWindow");
                }
                System.Diagnostics.Debug.WriteLine("GitHubLogin: Authentication successful");
                var userInfo = await FetchGitHubUserInfo(authResponse.OAuthToken);
                System.Diagnostics.Debug.WriteLine($"GitHubLogin: Fetched user info - Login: {userInfo.Login}, Name: {userInfo.Name}, Email: {userInfo.Email}");
                User? existingUser = await this.userService.GetUserByUsername(userInfo.Login);
                User user;
                bool isNewUser = false;
                if (existingUser == null)
                {
                    isNewUser = true;
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
                        byte[]? key = KeyGeneration.GenerateRandomKey(20);
                        user.TwoFASecret = Base32Encoding.ToString(key);
                        await this.userService.UpdateUser(user);
                    }
                }
                AuthenticationResponse? sessionResponse = await this.authenticationService.AuthWithOAuth(OAuthService.GitHub, gitHubOAuthHelper);
                System.Diagnostics.Debug.WriteLine($"Session creation response: {JsonSerializer.Serialize(sessionResponse)}");
                if (!sessionResponse.AuthenticationSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to create session for GitHub user");
                    ViewBag.ErrorMessage = "Failed to create session for GitHub user";
                    return View("MainWindow");
                }
                ViewBag.ShowQRCode = isNewUser;
                if (isNewUser)
                {
                    string qrCode = GenerateQRCode(user.Username, user.TwoFASecret);
                    ViewBag.QRCode = $"data:image/png;base64, {qrCode}";
                }
                ViewBag.Username = user.Username;
                System.Diagnostics.Debug.WriteLine($"GitHubLogin: Redirecting to TwoFactorAuthSetup for user {user.Username}");
                return View("TwoFactorAuthSetup");
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = $"GitHub authentication failed: {ex.Message}";
                return View("MainWindow");
            }
        }

        public async Task<IActionResult> FacebookLogin()
        {
            try
            {
                AuthenticationResponse? authResponse = await this.facebookOAuthHelper.AuthenticateAsync();
                if (!authResponse.AuthenticationSuccessful)
                {
                    ViewBag.ErrorMessage = "Facebook authentication failed";
                    return View("MainWindow");
                }
                var userInfo = await FetchFacebookUserInfo(authResponse.OAuthToken);
                User? existingUser = await this.userService.GetUserByUsername(userInfo.Login);
                User user;
                bool isNewUser = false;
                if (existingUser == null)
                {
                    isNewUser = true;
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
                    user = existingUser;
                    if (string.IsNullOrEmpty(user.TwoFASecret) || !IsValidBase32(user.TwoFASecret))
                    {
                        byte[]? key = KeyGeneration.GenerateRandomKey(20);
                        user.TwoFASecret = Base32Encoding.ToString(key);
                        await this.userService.UpdateUser(user);
                    }
                }
                ViewBag.ShowQRCode = isNewUser;
                if (isNewUser)
                {
                    string qrCode = GenerateQRCode(user.Username, user.TwoFASecret);
                    ViewBag.QRCode = $"data:image/png;base64, {qrCode}";
                }
                ViewBag.Username = user.Username;
                return View("TwoFactorAuthSetup");
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = $"Facebook authentication failed: {ex.Message}";
                return View("MainWindow");
            }
        }

        public async Task<IActionResult> LinkedInLogin()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LinkedInLogin: Starting LinkedIn authentication");
                AuthenticationResponse? authResponse = await this.linkedInOAuthHelper.AuthenticateAsync();
                System.Diagnostics.Debug.WriteLine($"LinkedInLogin: AuthenticationResponse - Success: {authResponse.AuthenticationSuccessful}, Token: {authResponse.OAuthToken}");
                if (!authResponse.AuthenticationSuccessful)
                {
                    System.Diagnostics.Debug.WriteLine("LinkedInLogin: Authentication failed");
                    ViewBag.ErrorMessage = "LinkedIn authentication failed";
                    return View("MainWindow");
                }
                var userInfo = await FetchLinkedInUserInfo(authResponse.OAuthToken);
                System.Diagnostics.Debug.WriteLine($"LinkedInLogin: Fetched user info - Login: {userInfo.Login}, Name: {userInfo.Name}, Email: {userInfo.Email}");
                User? existingUser = await this.userService.GetUserByUsername(userInfo.Login);
                User user;
                bool isNewUser = false;
                if (existingUser == null)
                {
                    isNewUser = true;
                    System.Diagnostics.Debug.WriteLine("LinkedInLogin: Creating new user");
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
                    user = existingUser;
                    System.Diagnostics.Debug.WriteLine("LinkedInLogin: User exists, updating if necessary");
                    if (string.IsNullOrEmpty(user.TwoFASecret) || !IsValidBase32(user.TwoFASecret))
                    {
                        System.Diagnostics.Debug.WriteLine("LinkedInLogin: Generating new TwoFASecret for existing user");
                        byte[]? key = KeyGeneration.GenerateRandomKey(20);
                        user.TwoFASecret = Base32Encoding.ToString(key);
                        await this.userService.UpdateUser(user);
                    }
                }
                ViewBag.ShowQRCode = isNewUser;
                if (isNewUser)
                {
                    string qrCode = GenerateQRCode(user.Username, user.TwoFASecret);
                    ViewBag.QRCode = $"data:image/png;base64, {qrCode}";
                }
                ViewBag.Username = user.Username;
                System.Diagnostics.Debug.WriteLine($"LinkedInLogin: Redirecting to TwoFactorAuthSetup for user {user.Username}");
                return View("TwoFactorAuthSetup");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LinkedIn authentication error: {ex.Message}");
                ViewBag.ErrorMessage = $"LinkedIn authentication failed: {ex.Message}";
                return View("MainWindow");
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

            HttpResponseMessage? userResponse = await client.GetAsync("https://api.github.com/user");
            userResponse.EnsureSuccessStatusCode();
            string? userResponseBody = await userResponse.Content.ReadAsStringAsync();
            JsonDocument? user = JsonDocument.Parse(userResponseBody);

            string? login = user.RootElement.GetProperty("login").GetString() ?? string.Empty;
            string? name = user.RootElement.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() ?? string.Empty : string.Empty;

            HttpResponseMessage? emailResponse = await client.GetAsync("https://api.github.com/user/emails");
            emailResponse.EnsureSuccessStatusCode();
            string? emailResponseBody = await emailResponse.Content.ReadAsStringAsync();
            JsonDocument? emails = JsonDocument.Parse(emailResponseBody);

            String? email = string.Empty;
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

        private async Task<(string Login, string Name, string Email)> FetchFacebookUserInfo(string accessToken)
        {
            using HttpClient? client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            HttpResponseMessage? userResponse = await client.GetAsync("https://graph.facebook.com/me?fields=id,name,email");
            userResponse.EnsureSuccessStatusCode();
            String? userResponseBody = await userResponse.Content.ReadAsStringAsync();
            JsonDocument? user = JsonDocument.Parse(userResponseBody);

            String? login = user.RootElement.GetProperty("id").GetString() ?? string.Empty;
            String? name = user.RootElement.GetProperty("name").GetString() ?? string.Empty;
            String? email = user.RootElement.TryGetProperty("email", out JsonElement emailProp) ? emailProp.GetString() ?? string.Empty : string.Empty;

            return (login, name, email);
        }

        public IActionResult Cancel2FA()
        {
            return RedirectToAction("MainWindow");
        }
        private async Task<(string Login, string Name, string Email)> FetchLinkedInUserInfo(string accessToken)
        {
            using HttpClient? client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            HttpResponseMessage? profileResponse = await client.GetAsync("https://api.linkedin.com/v2/me");
            profileResponse.EnsureSuccessStatusCode();
            String? profileJson = await profileResponse.Content.ReadAsStringAsync();
            JsonDocument? profile = JsonDocument.Parse(profileJson);

            String? id = profile.RootElement.GetProperty("id").GetString() ?? string.Empty;
            String? firstName = profile.RootElement.GetProperty("localizedFirstName").GetString() ?? string.Empty;
            String? lastName = profile.RootElement.GetProperty("localizedLastName").GetString() ?? string.Empty;
            String? name = $"{firstName} {lastName}".Trim();

            HttpResponseMessage? emailResponse = await client.GetAsync("https://api.linkedin.com/v2/emailAddress?q=members&projection=(elements*(handle~))");
            emailResponse.EnsureSuccessStatusCode();
            String? emailJson = await emailResponse.Content.ReadAsStringAsync();
            JsonDocument emailDoc = JsonDocument.Parse(emailJson);
            String? email = emailDoc.RootElement
                .GetProperty("elements")[0]
                .GetProperty("handle~")
                .GetProperty("emailAddress")
                .GetString() ?? string.Empty;

            return (id, name, email);
        }


        private bool VerifyTwoFactorCode(string twoFASecret, string enteredCode)
        {
            Totp? totp = new OtpNet.Totp(Base32Encoding.ToBytes(twoFASecret));
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