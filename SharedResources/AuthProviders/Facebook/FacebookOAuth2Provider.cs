using System.Text.Json;
using DataAccess.OAuthProviders;
using DataAccess.Model.Authentication;
using DataAccess.Model.AdminDashboard;
using DataAccess.Service.Authentication.Interfaces;
using DataAccess.Service.AdminDashboard.Interfaces;

namespace DataAccess.AuthProviders.Facebook
{
    public class FacebookOAuth2Provider : IGenericOAuth2Provider
    {
        private ISessionService sessionService;
        private IUserService userService;
        public FacebookOAuth2Provider(ISessionService sessionService, IUserService userService)
        {
            this.sessionService = sessionService;
            this.userService = userService;
        }

        public async Task<AuthenticationResponse> Authenticate(string userId, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://graph.facebook.com/me?fields=id,name,email&access_token={token}";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JsonElement doc = JsonDocument.Parse(json).RootElement;

                        Console.WriteLine(doc.ToString());

                        if (doc.TryGetProperty("id", out var idProp))
                        {
                            Console.WriteLine(idProp);
                            string fbId = idProp.GetString() ?? throw new Exception("Facebook ID is null.");
                            string fbName = doc.GetProperty("name").GetString() ?? throw new Exception("Facebook name is null.");

                            // Last time I checked, email wasn't sent
                            string email = "ionutcora66@gmail.com";

                            bool isNewAccount = await this.StoreOrUpdateUserInDb(fbId, fbName, email);

                            User? user = await userService.GetUserByUsername(fbName) ?? throw new Exception("User not found");

                            Session? session = await sessionService.CreateSessionAsync(user.UserId) ?? throw new Exception("Couldn't create session");

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
            User? user = await this.userService.GetUserByUsername(fbName);

            if (user == null)
            {
                await this.userService.CreateUser(new User
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
                await this.userService.UpdateUser(user);
                return false;
            }
        }
    }
}