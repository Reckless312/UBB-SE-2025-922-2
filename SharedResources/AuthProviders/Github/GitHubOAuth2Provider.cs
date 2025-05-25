using System.Text.Json;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;

namespace DataAccess.AuthProviders.Github
{
    public class GitHubOAuth2Provider : IGenericOAuth2Provider
    {
        private IUserService userService;
        private ISessionService sessionService;

        public GitHubOAuth2Provider(IUserService userService, ISessionService sessionService)
        {
            this.userService = userService;
            this.sessionService = sessionService;
        }

        public async Task<AuthenticationResponse> Authenticate(string? userId, string token)
        {
            return await this.AuthenticateAsync(userId, token);
        }

        private async Task<AuthenticationResponse> AuthenticateAsync(string? userId, string token)
        {
            try
            {
                (string gitHubId, string gitHubLogin, string email) = await this.FetchGitHubUserInfo(token);

                if (string.IsNullOrEmpty(gitHubLogin))
                {
                    throw new Exception("GitHub login is empty or null.");
                }

                User? user = await this.userService.GetUserByUsername(gitHubLogin);

                if (user == null)
                {
                    Guid newUserId = await this.CreateUserFromGitHub(gitHubLogin, email);
                    if (newUserId != Guid.Empty)
                    {
                        Session? session = await this.sessionService.CreateSessionAsync(newUserId);

                        if (session == null)
                        {
                            throw new Exception("Failed to create session for new user.");
                        }

                        return new AuthenticationResponse
                        {
                            AuthenticationSuccessful = true,
                            OAuthToken = token,
                            SessionId = session.SessionId,
                            NewAccount = true
                        };
                    }
                    else
                    {
                        throw new Exception("Go to catch");
                    }
                }
                else
                {
                    if (user.EmailAddress != email)
                    {
                        user.EmailAddress = email;
                        await this.userService.UpdateUser(user);
                    }

                    Session? session = await this.sessionService.CreateSessionAsync(user.UserId);

                    if (session == null)
                    {
                        throw new Exception("Failed to create session for user.");
                    }

                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = session.SessionId,
                        NewAccount = false
                    };
                }
            }
            catch (Exception)
            {
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = false,
                    OAuthToken = token,
                    SessionId = Guid.Empty,
                    NewAccount = false
                };
            }
        }

        private async Task<(string gitHubId, string gitHubLogin, string email)> FetchGitHubUserInfo(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");

                HttpResponseMessage userResponse = await client.GetAsync("https://api.github.com/user");
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch user info from GitHub. Status code: {userResponse.StatusCode}");
                }

                string userJson = await userResponse.Content.ReadAsStringAsync();
                using (JsonDocument userDoc = JsonDocument.Parse(userJson))
                {
                    JsonElement root = userDoc.RootElement;
                    string gitHubId = root.GetProperty("id").ToString();
                    string gitHubLogin = root.GetProperty("login").ToString();

                    HttpResponseMessage emailResponse = await client.GetAsync("https://api.github.com/user/emails");
                    if (!emailResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to fetch email from GitHub. Status code: {emailResponse.StatusCode}");
                    }

                    string emailJson = await emailResponse.Content.ReadAsStringAsync();
                    using (JsonDocument emailDoc = JsonDocument.Parse(emailJson))
                    {
                        JsonElement.ArrayEnumerator emails = emailDoc.RootElement.EnumerateArray();
                        string primaryEmail = emails.FirstOrDefault(e => e.GetProperty("primary").GetBoolean()).GetProperty("email").GetString()
                            ?? throw new Exception("No primary email found for GitHub user");

                        return (gitHubId, gitHubLogin, primaryEmail);
                    }
                }
            }
        }

        private async Task<Guid> CreateUserFromGitHub(string gitHubLogin, string email)
        {
            try
            {
                User newUser = new ()
                {
                    UserId = Guid.NewGuid(),
                    Username = gitHubLogin.Trim(),
                    TwoFASecret = string.Empty,
                    PasswordHash = string.Empty,
                    EmailAddress = email,
                    NumberOfDeletedReviews = 0,
                    HasSubmittedAppeal = false,
                    AssignedRole = RoleType.User,
                    FullName = gitHubLogin.Trim()
                };
                await this.userService.CreateUser(newUser);
                return newUser.UserId;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error creating user: " + exception.Message);
            }
            return Guid.Empty;
        }
    }
}