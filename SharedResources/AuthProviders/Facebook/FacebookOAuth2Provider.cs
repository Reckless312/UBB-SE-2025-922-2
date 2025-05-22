using System;
using System.Net.Http;
using System.Text.Json;
using DataAccess.OAuthProviders;
using DataAccess.Model.Authentication;
using IRepository;
using Repository.AdminDashboard;
using Repository.Authentication;
using DataAccess.Model.AdminDashboard;
using System.Collections.Generic;
using DataAccess.Service.Authentication.Interfaces;
using DataAccess.Service.AdminDashboard.Interfaces;

namespace DataAccess.AuthProviders.Facebook
{
    public class FacebookOAuth2Provider : GenericOAuth2Provider
    {
        private ISessionService sessionService;
        private IUserService userService;
        public FacebookOAuth2Provider(ISessionService sessionService, IUserService userService)
        {
            this.sessionService = sessionService;
            this.userService = userService;
        }

        public AuthenticationResponse Authenticate(string userId, string token)
        {
            Console.WriteLine("User id: " + userId);
            Console.WriteLine("Token:" + token);
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={token}";
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("I got a successful code");
                        string json = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine(json);
                        var doc = JsonDocument.Parse(json).RootElement;

                        Console.WriteLine(doc.ToString());

                        if (doc.TryGetProperty("id", out var idProp))
                        {
                            Console.WriteLine(idProp);
                            string fbId = idProp.GetString() ?? throw new Exception("Facebook ID is null.");
                            string fbName = doc.GetProperty("name").GetString() ?? throw new Exception("Facebook name is null.");
                            string email = "ionutcora66@gmail.com";

                            // store or update user in DB - UserService
                            bool isNewAccount = StoreOrUpdateUserInDb(fbId, fbName, email).Result;

                            User user = userService.GetUserByUsername(fbName).Result ?? throw new Exception("User not found");

                            Session session = sessionService.CreateSessionAsync(user.UserId).Result;

                            if (isNewAccount)
                            {
                                return new AuthenticationResponse
                                {
                                    AuthenticationSuccessful = true,
                                    SessionId = session.SessionId,
                                    OAuthToken = token,
                                    NewAccount = true
                                };
                            }
                            else
                            {
                                return new AuthenticationResponse
                                {
                                    AuthenticationSuccessful = true,
                                    SessionId = session.SessionId,
                                    OAuthToken = token,
                                    NewAccount = false
                                };
                            }
                        }
                    }
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = token,
                        SessionId = Guid.Empty,
                        NewAccount = false
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

        private async Task<bool> StoreOrUpdateUserInDb(string fbId, string fbName, string email)
        {
            User? user;
            try
            {
                user = userService.GetUserByUsername(fbName).Result;
            }
            catch (Exception ex)
            {
                user = null;
            }

            if (user == null)
            {
                await userService.CreateUser(new User
                {
                    Username = fbName,
                    PasswordHash = string.Empty,
                    UserId = Guid.NewGuid(),
                    TwoFASecret = string.Empty,
                    EmailAddress = email,
                    NumberOfDeletedReviews = 0,
                    HasSubmittedAppeal = false,
                    AssignedRole = RoleType.User,
                    FullName = fbName
                });
                return true;
            }
            else
            {
                user.Username = fbName;
                user.EmailAddress = email;
                await userService.UpdateUser(user);
                return false;
            }
        }
    }
}