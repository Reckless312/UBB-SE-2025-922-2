namespace DrinkDb_Auth.AuthProviders.Google
{
    using System.Net.Http.Json;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json.Serialization;
    using DataAccess.Model.Authentication;
    using DataAccess.OAuthProviders;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using DataAccess.Service.Authentication.Interfaces;

    public class GoogleOAuth2Provider : IGenericOAuth2Provider, IGoogleOAuth2Provider
    {
        public static Guid CreateGloballyUniqueIdentifier(string identifier)
        {
            using (MD5 cryptographicHasher = MD5.Create())
            {
                byte[] hashResult = cryptographicHasher.ComputeHash(Encoding.UTF8.GetBytes(identifier));
                return new Guid(hashResult);
            }
        }

        private string ClientId { get; }
        private string ClientSecret { get; }

        private string RedirectUniformResourceIdentifier { get; }
        private string AuthorizationEndpoint { get; }
        private string TokenEndpoint { get; }
        private string UserInformationEndpoint { get; }

        private readonly string[] userResourcesScope = { "profile", "email" };
        private HttpClient httpClient;
        private ISessionService sessionService;
        private IUserService userService;

        public GoogleOAuth2Provider(ISessionService sessionService, IUserService userService)
        {
            this.httpClient = new HttpClient();
            this.sessionService = sessionService;
            this.userService = userService;

            this.ClientId = "311954949107-k5agbsvuvrsuttupcu7av2lceuk4vlag.apps.googleusercontent.com";
            this.ClientSecret = "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";
            this.RedirectUniformResourceIdentifier = "urn:ietf:wg:oauth:2.0:oob";
            this.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
            this.TokenEndpoint = "https://oauth2.googleapis.com/token";
            this.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        }

        private async Task<Guid> EnsureUserExists(string identifier, string email, string name)
        {
            User? user = await this.userService.GetUserByUsername(name);
            Guid userId;
            if (user == null)
            {
                userId = Guid.NewGuid();
                User newUser = new User { UserId = userId, Username = email, PasswordHash = string.Empty, TwoFASecret = null, EmailAddress = email, FullName = name, AssignedRole = DataAccess.Model.AdminDashboard.RoleType.User };
                await this.userService.CreateUser(newUser);
            }
            else
            {
                userId = user.UserId;
                if (user.EmailAddress != email)
                {
                    user.EmailAddress = email;
                    await this.userService.UpdateUser(user);
                }
            }
            return userId;
        }

        public string GetAuthorizationUrl()
        {
            string allowedResourcesScope = string.Join(" ", this.userResourcesScope);

            Dictionary<string, string> authorizationData = new Dictionary<string, string>
            {
                { "client_id", this.ClientId },
                { "redirect_uri", this.RedirectUniformResourceIdentifier },
                { "response_type", "code" },
                { "scope", allowedResourcesScope },
                { "access_type", "offline" },
                { "state", Guid.NewGuid().ToString() }
            };

            string transformedURLData = string.Join("&", authorizationData.Select(row => $"{Uri.EscapeDataString(row.Key)}={Uri.EscapeDataString(row.Value)}"));
            string fullAuthorizationURL = $"{this.AuthorizationEndpoint}?{transformedURLData}";

            return fullAuthorizationURL;
        }

        public async Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code)
        {
            Dictionary<string, string> tokenRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "redirect_uri", RedirectUniformResourceIdentifier },
                { "grant_type", "authorization_code" }
            };

            // Whoever wrote this many nested catches, I genuinelly hate you :<
            try
            {
                FormUrlEncodedContent formatContent = new FormUrlEncodedContent(tokenRequest);
                HttpResponseMessage tokenResponse = await httpClient.PostAsync(TokenEndpoint, formatContent);
                string responseContent = await tokenResponse.Content.ReadAsStringAsync();

                switch (tokenResponse.IsSuccessStatusCode)
                {
                    case true:
                        TokenResponse? tokenResult = null;

                        try
                        {
                            tokenResult = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
                        }
                        catch
                        {
                        }

                        System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                        try
                        {
                            tokenResult = tokenResult == null ? System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseContent, options) : tokenResult;
                        }
                        catch
                        {
                        }

                        if (tokenResult == null || string.IsNullOrEmpty(tokenResult.AccessToken))
                        {
                            return new AuthenticationResponse { AuthenticationSuccessful = false, OAuthToken = tokenResult?.AccessToken, SessionId = Guid.Empty, NewAccount = false };
                        }

                        UserInfoResponse userInformation;
                        Guid userId;
                        try
                        {
                            using (HttpClient httpClient = new HttpClient())
                            {
                                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
                                await Task.Delay(500);
                                HttpResponseMessage? httpClientResponse = await httpClient.GetAsync(UserInformationEndpoint);
                                string httpClientResponseContent = await httpClientResponse.Content.ReadAsStringAsync();

                                switch (httpClientResponse.IsSuccessStatusCode)
                                {
                                    case true:
                                        UserInfoResponse? httpClientInformation = await httpClientResponse.Content.ReadFromJsonAsync<UserInfoResponse>();

                                        if (httpClientInformation == null)
                                        {
                                            throw new Exception("Couldn't get http client informatin");
                                        }

                                        userInformation = ExtractUserInfoFromIdToken(tokenResult.IdToken);
                                        userId = await EnsureUserExists(userInformation.Identifier, httpClientInformation.Email, httpClientInformation.Name);

                                        Session? session = await sessionService.CreateSessionAsync(userId);

                                        if (session == null)
                                        {
                                            throw new Exception("Couldn't create session for user");
                                        }

                                        return new AuthenticationResponse { AuthenticationSuccessful = true, OAuthToken = tokenResult.AccessToken, SessionId = session.SessionId, NewAccount = false };
                                    case false:
                                        if (string.IsNullOrEmpty(tokenResult.IdToken))
                                        {
                                            return new AuthenticationResponse { AuthenticationSuccessful = true, OAuthToken = tokenResult.AccessToken, SessionId = Guid.Empty, NewAccount = false };
                                        }
                                        else
                                        {
                                            throw new Exception("Trigger Catch | Repeated code to attempt a succesfull authentication");
                                        }
                                }
                            }
                        }
                        catch
                        {
                            userInformation = ExtractUserInfoFromIdToken(tokenResult.IdToken);
                            userId = await EnsureUserExists(userInformation.Identifier, userInformation.Email, userInformation.Name);

                            Session? session = await sessionService.CreateSessionAsync(userId);

                            if (session == null)
                            {
                                throw new Exception("Couldn't create session for user");
                            }

                            return new AuthenticationResponse { AuthenticationSuccessful = true, OAuthToken = tokenResult.AccessToken, SessionId = session.SessionId, NewAccount = false };
                        }
                    case false:
                        throw new Exception("Trigger Catch | Repeated code to attempt a failed authentication");
                }
            }
            catch
            {
                return new AuthenticationResponse { AuthenticationSuccessful = false, OAuthToken = string.Empty, SessionId = Guid.Empty, NewAccount = false };
            }
        }

        private UserInfoResponse ExtractUserInfoFromIdToken(string idToken)
        {
            // Too many random numbers and chars to even pretend I know what's happening
            string[] splittedToken = idToken.Split('.');
            if (splittedToken.Length != 3)
            {
                throw new ArgumentException("Invalid JWT format");
            }

            try
            {
                int payloadIndex = 1;
                string payload = splittedToken[payloadIndex];

                while (payload.Length % 4 != 0)
                {
                    payload += '=';
                }

                byte[] jsonInBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
                string json = Encoding.UTF8.GetString(jsonInBytes);

                System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                UserInfoResponse? result = System.Text.Json.JsonSerializer.Deserialize<UserInfoResponse>(json, options);
                return result != null ? result : throw new Exception("Failed to deserialize user info from ID token");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing ID token: {ex.Message}", ex);
            }
        }

        public Task<AuthenticationResponse> Authenticate(string userId, string token)
        {
            return Task.FromResult(new AuthenticationResponse { AuthenticationSuccessful = !string.IsNullOrEmpty(token), OAuthToken = token, SessionId = Guid.Empty, NewAccount = false });
        }

        internal class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public required string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public required int ExpiresIn { get; set; }

            [JsonPropertyName("refresh_token")]
            public required string RefreshToken { get; set; }

            [JsonPropertyName("id_token")]
            public required string IdToken { get; set; }
        }

        internal class UserInfoResponse
        {
            [JsonPropertyName("sub")]
            public required string Identifier { get; set; }

            [JsonPropertyName("name")]
            public required string Name { get; set; }

            [JsonPropertyName("given_name")]
            public required string GivenName { get; set; }

            [JsonPropertyName("family_name")]
            public required string FamilyName { get; set; }

            [JsonPropertyName("picture")]
            public required string Picture { get; set; }

            [JsonPropertyName("email")]
            public required string Email { get; set; }

            [JsonPropertyName("email_verified")]
            public required bool EmailVerified { get; set; }
        }
    }
}