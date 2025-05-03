using System;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.Repository.AdminDashboard;
using IRepository;
using DrinkDb_Auth.Repository.Authentication;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Data.SqlClient;
using System.Configuration;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public class GitHubOAuth2Provider : GenericOAuth2Provider
    {
        private IUserRepository userRepository;
        private ISessionRepository sessionRepository;
        private IGitHubHttpHelper gitHubHttpHelper;

        public GitHubOAuth2Provider()
        {
            userRepository = new UserRepository();
            sessionRepository = new SessionRepository();
            gitHubHttpHelper = new GitHubHttpHelper();
        }

        public GitHubOAuth2Provider(IUserRepository userRepository, ISessionRepository sessionAdapter, IGitHubHttpHelper gitHubHttpHelper)
        {
            this.userRepository = userRepository;
            this.sessionRepository = sessionAdapter;
            this.gitHubHttpHelper = gitHubHttpHelper;
        }

        public AuthenticationResponse Authenticate(string? userId, string token)
        {
            try
            {
                var (gitHubId, gitHubLogin, email) = FetchGitHubUserInfo(token);

                if (string.IsNullOrEmpty(gitHubLogin))
                {
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    };
                }

                // Check if a user exists by using the GitHub username.
                if (UserExists(gitHubLogin))
                {
                    // User exists, so proceed.
                    User user = userRepository.GetUserByUsername(gitHubLogin) ?? throw new Exception("User not found");

                    // Update email if it's different
                    if (user.EmailAddress != email)
                    {
                        user.EmailAddress = email;
                        userRepository.UpdateUser(user);
                    }

                    Session session = sessionRepository.CreateSession(user.UserId);

                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = session.SessionId,
                        NewAccount = false
                    };
                }
                else
                {
                    // User does not exist. Insert the new user.
                    Guid newUserId = CreateUserFromGitHub(gitHubLogin, email);
                    if (newUserId != Guid.Empty)
                    {
                        // Successfully inserted, so login is successful.
                        Session session = sessionRepository.CreateSession(newUserId);
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
                        // Insertion failed.
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

        private (string gitHubId, string gitHubLogin, string email) FetchGitHubUserInfo(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");

                // First get the user info
                var userResponse = client.GetAsync("https://api.github.com/user").Result;
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch user info from GitHub. Status code: {userResponse.StatusCode}");
                }

                string userJson = userResponse.Content.ReadAsStringAsync().Result;
                using (JsonDocument userDoc = JsonDocument.Parse(userJson))
                {
                    var root = userDoc.RootElement;
                    string gitHubId = root.GetProperty("id").ToString();
                    string gitHubLogin = root.GetProperty("login").ToString();

                    // Then get the user's email
                    var emailResponse = client.GetAsync("https://api.github.com/user/emails").Result;
                    if (!emailResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to fetch email from GitHub. Status code: {emailResponse.StatusCode}");
                    }

                    string emailJson = emailResponse.Content.ReadAsStringAsync().Result;
                    using (JsonDocument emailDoc = JsonDocument.Parse(emailJson))
                    {
                        var emails = emailDoc.RootElement.EnumerateArray();
                        string primaryEmail = emails.FirstOrDefault(e => e.GetProperty("primary").GetBoolean()).GetProperty("email").GetString()
                            ?? throw new Exception("No primary email found for GitHub user");

                        return (gitHubId, gitHubLogin, primaryEmail);
                    }
                }
            }
        }

        private bool UserExists(string gitHubLogin)
        {
            try
            {
                User? user = userRepository.GetUserByUsername(gitHubLogin);
                if (user != null)
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error verifying user: " + exception.Message);
            }
            return false;
        }

        private Guid CreateUserFromGitHub(string gitHubLogin, string email)
        {
            try
            {
                User newUser = new()
                {
                    UserId = Guid.NewGuid(),
                    Username = gitHubLogin.Trim(),
                    TwoFASecret = string.Empty,
                    PasswordHash = string.Empty,
                    EmailAddress = email,
                };
                userRepository.CreateUser(newUser);
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