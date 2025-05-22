using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;
using IRepository;
using Microsoft.Data.SqlClient;
using Repository.AdminDashboard;
using Repository.Authentication;

namespace DataAccess.AuthProviders.LinkedIn
{
    public class LinkedInOAuth2Provider : GenericOAuth2Provider
    {
        private IUserService userService;
        private ISessionService sessionService;

        public LinkedInOAuth2Provider(IUserService userService, ISessionService sessionService)
        {
            this.userService = userService;
            this.sessionService = sessionService;
        }

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
            User? user;
            try
            {
                user = userService.GetUserByUsername(name).Result;
            }
            catch(Exception ex)
            {
                user = null;
            }
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
                Session session = sessionService.CreateSessionAsync(newUser.UserId).Result;
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
                    bool updated = userService.UpdateUser(user).Result;
                }

                Session session = sessionService.CreateSessionAsync(user.UserId).Result;
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = true,
                    OAuthToken = token,
                    SessionId = session.SessionId,
                    NewAccount = false,
                };
            }
        }
    }
}


