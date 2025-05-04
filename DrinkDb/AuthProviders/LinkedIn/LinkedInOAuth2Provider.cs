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

namespace DrinkDb_Auth.AuthProviders.LinkedIn
{
    public class LinkedInOAuth2Provider : GenericOAuth2Provider
    {

        private const string LinkedInIdField = "sub";
        private const string LinkedInNameField = "name";
        private const string LinkedInEmailField = "email";
        private const int INITIAL_DELETED_REVIEWS = 0;


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
            HttpResponseMessage response = client.GetAsync("https://api.linkedin.com/v2/userinfo").Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to fetch user info from Linkedin.");
            }

            string json = response.Content.ReadAsStringAsync().Result;

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            string linkedinId = root.GetProperty(LinkedInIdField).GetString() ?? throw new Exception("LinkedIn ID not found in response.");
            string linkedinName = root.GetProperty(LinkedInNameField).GetString() ?? throw new Exception("LinkedIn name not found in response.");

            if (string.IsNullOrEmpty(linkedinId) || string.IsNullOrEmpty(linkedinName))
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

            User user = UserRepository.GetUserByUsername(linkedinName);
            if (user == null)
            {
                User newUser = new User
                {
                    Username = linkedinName,
                    PasswordHash = string.Empty,
                    UserId = Guid.NewGuid(),
                    TwoFASecret = string.Empty,
                    EmailAddress = root.GetProperty(LinkedInEmailField).GetString() ?? string.Empty,
                    NumberOfDeletedReviews = INITIAL_DELETED_REVIEWS,
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
