using System.Diagnostics;
using System.Text.Json;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;

namespace DataAccess.AuthProviders.LinkedIn
{
    public class LinkedInOAuth2Provider : IGenericOAuth2Provider
    {
        private IUserService userService;
        private ISessionService sessionService;

        public LinkedInOAuth2Provider(IUserService userService, ISessionService sessionService)
        {
            this.userService = userService;
            this.sessionService = sessionService;
        }

        public async Task<AuthenticationResponse> Authenticate(string userId, string token)
        {
            try
            {
                using HttpClient client = new ();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");
                HttpResponseMessage response = await client.GetAsync("https://api.linkedin.com/v2/userinfo");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch user info from Linkedin.");
                }

                string json = await response.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;
                string id = root.GetProperty("sub").GetString() ?? throw new Exception("LinkedIn ID not found in response.");
                string name = root.GetProperty("name").GetString() ?? throw new Exception("LinkedIn name not found in response.");

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                {
                    Debug.WriteLine("LinkedIn ID or name is empty.");
                    throw new Exception("Go to catch");
                }
                User? user = await userService.GetUserByUsername(name);
                if (user == null)
                {
                    User newUser = new User
                    {
                        Username = name,
                        PasswordHash = string.Empty,
                        UserId = Guid.NewGuid(),
                        TwoFASecret = string.Empty,
                        EmailAddress = root.GetProperty("email").GetString() ?? string.Empty,
                        NumberOfDeletedReviews = 0,
                        HasSubmittedAppeal = false,
                        AssignedRole = RoleType.User,
                        FullName = name,
                    };

                    bool created = this.userService.CreateUser(newUser).Result;
                    Session? session = await sessionService.CreateSessionAsync(newUser.UserId);

                    if (session == null)
                    {
                        throw new Exception("Failed to create session for new user.");
                    }

                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = session.SessionId,
                        NewAccount = true,
                    };
                }
                else
                {
                    string email = root.GetProperty("email").GetString() ?? string.Empty;
                    if (user.EmailAddress != email)
                    {
                        user.EmailAddress = email;
                        bool updated = await this.userService.UpdateUser(user);
                    }

                    Session? session = await sessionService.CreateSessionAsync(user.UserId);

                    if (session == null)
                    {
                        throw new Exception("Failed to create session for existing user.");
                    }

                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = session.SessionId,
                        NewAccount = false,
                    };
                }
            }
            catch
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
    }
}


