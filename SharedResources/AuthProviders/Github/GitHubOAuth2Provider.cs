using System;
using DataAccess.Model.AdminDashboard;
using System.Collections.Generic;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using IRepository;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using Repository.AdminDashboard;
using Repository.Authentication;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;

namespace DataAccess.AuthProviders.Github
{
    public class GitHubOAuth2Provider : GenericOAuth2Provider
    {
        private IUserService userService;
        private ISessionService sessionService;

        public GitHubOAuth2Provider(IUserService userService, ISessionService sessionService)
        {
            this.userService = userService;
            this.sessionService = sessionService;
        }

        public AuthenticationResponse Authenticate(string? userId, string token)
        {
            return AuthenticateAsync(userId, token).Result;
        }

        private async Task<AuthenticationResponse> AuthenticateAsync(string? userId, string token)
        {
            try
            {
                var (gitHubId, gitHubLogin, email) = await FetchGitHubUserInfo(token);

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
                if (await UserExists(gitHubLogin))
                {
                    // User exists, so proceed.
                    User user = userService.GetUserByUsername(gitHubLogin).Result ?? throw new Exception("User not found");

                    if (user.EmailAddress != email)
                    {
                        user.EmailAddress = email;
                        await userService.UpdateUser(user) ;
                    }

                    Session session = sessionService.CreateSessionAsync(user.UserId).Result;

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
                        Session session = sessionService.CreateSessionAsync(newUserId).Result;
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

        private async Task<(string gitHubId, string gitHubLogin, string email)> FetchGitHubUserInfo(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");

                // First get the user info
                var userResponse = await client.GetAsync("https://api.github.com/user");
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch user info from GitHub. Status code: {userResponse.StatusCode}");
                }

                string userJson = await userResponse.Content.ReadAsStringAsync();
                using (JsonDocument userDoc = JsonDocument.Parse(userJson))
                {
                    var root = userDoc.RootElement;
                    string gitHubId = root.GetProperty("id").ToString();
                    string gitHubLogin = root.GetProperty("login").ToString();

                    // Then get the user's email
                    var emailResponse = await client.GetAsync("https://api.github.com/user/emails");
                    if (!emailResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to fetch email from GitHub. Status code: {emailResponse.StatusCode}");
                    }

                    string emailJson = await emailResponse.Content.ReadAsStringAsync();
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

        private async Task<bool> UserExists(string gitHubLogin)
        {
            try
            {
                User? user = await userService.GetUserByUsername(gitHubLogin);
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
                    NumberOfDeletedReviews = 0,
                    HasSubmittedAppeal = false,
                    AssignedRole = RoleType.Manager,
                    FullName = gitHubLogin.Trim()
                };
                userService.CreateUser(newUser);
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