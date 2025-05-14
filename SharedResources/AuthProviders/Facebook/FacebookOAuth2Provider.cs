using System;
using System.Net.Http;
using System.Text.Json;
using DataAccess.OAuthProviders;
using Windows.Networking.Sockets;
using DataAccess.Model.Authentication;
using IRepository;
using Repository.AdminDashboard;
using Repository.Authentication;
using DataAccess.Model.AdminDashboard;
using System.Collections.Generic;

namespace DataAccess.AuthProviders.Facebook
{
    public class FacebookOAuth2Provider : GenericOAuth2Provider
    {
        private ISessionRepository
        public FacebookOAuth2Provider()
        {

        }

        private static readonly SessionRepository SessionProxy = new ();
        public AuthenticationResponse Authenticate(string userId, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={token}";
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        var doc = JsonDocument.Parse(json).RootElement;

                        if (doc.TryGetProperty("id", out var idProp))
                        {
                            string fbId = idProp.GetString() ?? throw new Exception("Facebook ID is null.");
                            string fbName = doc.GetProperty("name").GetString() ?? throw new Exception("Facebook name is null.");
                            string email = doc.GetProperty("email").GetString() ?? string.Empty;

                            // store or update user in DB - UserService
                            bool isNewAccount = StoreOrUpdateUserInDb(fbId, fbName, email);

                            User user = UserRepository.GetUserByUsername(fbName).Result ?? throw new Exception("User not found");

                            Session session = SessionProxy.CreateSession(user.UserId).Result;

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
        private static readonly IUserRepository UserRepository = new UserRepository();
        private bool StoreOrUpdateUserInDb(string fbId, string fbName, string email)
        {
            User user = UserRepository.GetUserByUsername(fbName).Result;

            if (user == null)
            {
                UserRepository.CreateUser(new User
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
                UserRepository.UpdateUser(user);
                return false;
            }
        }
    }
}