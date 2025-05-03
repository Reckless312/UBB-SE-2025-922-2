using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.Repository.AdminDashboard;
using IRepository;
using DrinkDb_Auth.Repository.Authentication;
using Microsoft.Data.SqlClient;

namespace DrinkDb_Auth.AuthProviders.LinkedIn
{
    public class LinkedInOAuth2Provider : GenericOAuth2Provider
    {
        private readonly static IUserRepository UserRepository = new UserRepository();
        private readonly static SessionRepository SessionAdapter = new();

        /// <summary>
        /// Performs authentication using the access token, fetches user info via OpenID Connect, and stores/updates the user.
        /// </summary>
        public AuthenticationResponse Authenticate(string userId, string token)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");
            var response = client.GetAsync("https://api.linkedin.com/v2/userinfo").Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch user info from Linkedin.");
            }

            string json = response.Content.ReadAsStringAsync().Result;

            using JsonDocument document = JsonDocument.Parse(json);
            var root = document.RootElement;
            string id = root.GetProperty("sub").GetString() ?? throw new Exception("LinkedIn ID not found in response.");
            string name = root.GetProperty("name").GetString() ?? throw new Exception("LinkedIn name not found in response.");

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                Debug.WriteLine("LinkedIn ID or name is empty.");
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = false,
                    OAuthToken = string.Empty,
                    SessionId = Guid.Empty,
                    NewAccount = false
                };
            }

            var user = UserRepository.GetUserByUsername(name);
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
                    HasSubmittedAppeal = false
                };
                UserRepository.CreateUser(newUser);
                Session session = SessionAdapter.CreateSession(newUser.UserId);
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
                // Update email if it's different
                string email = root.GetProperty("email").GetString() ?? string.Empty;
                if (user.EmailAddress != email)
                {
                    user.EmailAddress = email;
                    UserRepository.UpdateUser(user);
                }
                Session session = SessionAdapter.CreateSession(user.UserId);
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = true,
                    OAuthToken = token,
                    SessionId = session.SessionId,
                    NewAccount = false
                };
            }
        }
    }
}
