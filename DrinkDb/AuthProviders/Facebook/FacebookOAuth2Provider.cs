using System;
using System.Net.Http;
using System.Text.Json;
using DrinkDb_Auth.OAuthProviders;
using Windows.Networking.Sockets;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.Repository.Authentication;
using IRepository;
using DrinkDb_Auth.Repository.AdminDashboard;

namespace DrinkDb_Auth.AuthProviders.Facebook
{
    public class FacebookOAuth2Provider : GenericOAuth2Provider
    {
        private readonly IUserRepository userRepository;
        private readonly ISessionRepository sessionRepository;

        public FacebookOAuth2Provider()
        {
            userRepository = new UserRepository();
            sessionRepository = new SessionRepository();
        }

        public FacebookOAuth2Provider(IUserRepository userRepository, ISessionRepository sessionRepository)
        {
            this.userRepository = userRepository;
            this.sessionRepository = sessionRepository;
        }

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

                            // Check if user exists
                            var user = userRepository.GetUserByUsername(fbName);
                            if (user == null)
                            {
                                // Create new user
                                User newUser = new()
                                {
                                    UserId = Guid.NewGuid(),
                                    Username = fbName,
                                    PasswordHash = string.Empty,
                                    TwoFASecret = string.Empty,
                                    EmailAddress = email
                                };
                                userRepository.CreateUser(newUser);
                                Session session = sessionRepository.CreateSession(newUser.UserId);

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
    }
}