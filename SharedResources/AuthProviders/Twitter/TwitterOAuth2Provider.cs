namespace DataAccess.AuthProviders.Twitter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DataAccess.OAuthProviders;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using DataAccess.Service.Authentication.Interfaces;

    public class TwitterOAuth2Provider : ITwitterOAuth2Provider
    {
        private IUserService userService;
        private ISessionService sessionService;

        private string ClientId { get; }
        private string ClientSecret { get; }

        private const string RedirectUri = "http://127.0.0.1:5000/x-callback";

        private const string AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
        private const string UserInfoEndpoint = "https://api.twitter.com/2/users/me";

        private readonly string[] scopes = { "tweet.read", "users.read" };

        public string codeVerifier = string.Empty;

        private readonly HttpClient httpClient;

        public TwitterOAuth2Provider(IUserService userService, ISessionService sessionService)
        {
            this.httpClient = new HttpClient();

            this.ClientId = "ODVNN2VYRGR4ZWNfcm9LQnlzS2Q6MTpjaQ";
            this.ClientSecret = "B7eMoprWDmTGzsYz - 3kK8Hsqpc5oJ4i4Gt9tjqtFb73J5dBQyz";

            this.userService = userService;
            this.sessionService = sessionService;
        }

        public AuthenticationResponse Authenticate(string userId, string token)
        {
            return new AuthenticationResponse
            {
                AuthenticationSuccessful = !string.IsNullOrEmpty(token),
                OAuthToken = token,
                SessionId = Guid.Empty,
                NewAccount = false
            };
        }

        public string GetAuthorizationUrl()
        {
            (string generatedCodeVerifier, string generatedCodeChallenge) = this.GeneratePkceData();
            this.codeVerifier = generatedCodeVerifier;

            string concatenatedScopes = string.Join(" ", this.scopes);

            Dictionary<string, string> authorizationParameters = new Dictionary<string, string>
            {
                { "client_id", this.ClientId },
                { "redirect_uri", TwitterOAuth2Provider.RedirectUri },
                { "response_type", "code" },
                { "scope", concatenatedScopes },
                { "state", Guid.NewGuid().ToString() },
                { "code_challenge", generatedCodeChallenge },
                { "code_challenge_method", "S256" }
            };

            string encodedQueryString = string.Join("&", authorizationParameters
                .Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"));

            string fullAuthorizationUrl = $"{TwitterOAuth2Provider.AuthorizationEndpoint}?{encodedQueryString}";
            return fullAuthorizationUrl;
        }

        public async Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code)
        {
            Dictionary<string, string> tokenRequestParameters = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", this.ClientId },
                { "redirect_uri", TwitterOAuth2Provider.RedirectUri },
                { "grant_type", "authorization_code" },
                { "code_verifier", this.codeVerifier },
            };

            try
            {
                using FormUrlEncodedContent requestContent = new FormUrlEncodedContent(tokenRequestParameters);
                HttpResponseMessage tokenResponse = await httpClient.PostAsync(TwitterOAuth2Provider.TokenEndpoint, requestContent);
                string tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    };
                }

                TwitterTokenResponse? twitterTokenResult;
                try
                {
                    twitterTokenResult = await tokenResponse.Content.ReadFromJsonAsync<TwitterTokenResponse>();
                }
                catch
                {
                    twitterTokenResult = System.Text.Json.JsonSerializer.Deserialize<TwitterTokenResponse>(tokenResponseContent);
                }

                if (twitterTokenResult == null || string.IsNullOrEmpty(twitterTokenResult.AccessToken))
                {
                    System.Diagnostics.Debug.WriteLine("No access token in tokenResult.");
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    };
                }

                try
                {
                    using HttpClient twitterUserInfoClient = new HttpClient();
                    twitterUserInfoClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", twitterTokenResult.AccessToken);

                    HttpResponseMessage userInfoResponse = await twitterUserInfoClient.GetAsync(TwitterOAuth2Provider.UserInfoEndpoint);
                    string userInfoResponseBody = await userInfoResponse.Content.ReadAsStringAsync();

                    if (!userInfoResponse.IsSuccessStatusCode)
                    {
                        return new AuthenticationResponse
                        {
                            AuthenticationSuccessful = false,
                            OAuthToken = twitterTokenResult.AccessToken,
                            SessionId = Guid.Empty,
                            NewAccount = false
                        };
                    }

                    TwitterUserInfoResponse? twitterUserInfoObject = System.Text.Json.JsonSerializer.Deserialize<TwitterUserInfoResponse>(userInfoResponseBody);

                    if (twitterUserInfoObject == null || twitterUserInfoObject.Data == null || twitterUserInfoObject.Data.Username == null)
                    {
                        return new AuthenticationResponse
                        {
                            AuthenticationSuccessful = false,
                            OAuthToken = twitterTokenResult.AccessToken,
                            SessionId = Guid.Empty,
                            NewAccount = false
                        };
                    }

                    User? user = await userService.GetUserByUsername(twitterUserInfoObject.Data.Username);
                    if (user == null)
                    {
                        user = new User
                        {
                            Username = twitterUserInfoObject.Data.Username,
                            PasswordHash = string.Empty,
                            UserId = Guid.NewGuid(),
                            TwoFASecret = string.Empty,
                            EmailAddress = "cevaemail",
                            NumberOfDeletedReviews = 0,
                            HasSubmittedAppeal = false,
                            AssignedRole = RoleType.Admin,
                            FullName = twitterUserInfoObject.Data.Username,
                        };
                        await this.userService.CreateUser(user);
                    }
                    else
                    {
                        await this.userService.UpdateUser(user);
                    }

                    Session? userSession = await this.sessionService.CreateSessionAsync(user.UserId);

                    if (userSession == null)
                    {
                        return new AuthenticationResponse
                        {
                            AuthenticationSuccessful = false,
                            OAuthToken = twitterTokenResult.AccessToken,
                            SessionId = Guid.Empty,
                            NewAccount = false
                        };
                    }

                    return new AuthenticationResponse
                    {
                        OAuthToken = twitterTokenResult.AccessToken,
                        AuthenticationSuccessful = true,
                        SessionId = userSession.SessionId,
                        NewAccount = false
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching user info: {ex.Message}");
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
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
                    OAuthToken = string.Empty,
                    SessionId = Guid.Empty,
                    NewAccount = false,
                };
            }
        }

        public string ExtractQueryParameter(string fullUrl, string targetParameter)
        {
            Uri uriObject = new Uri(fullUrl);
            string queryString = uriObject.Query.TrimStart('?');
            string[] parameterPairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (string parameterPair in parameterPairs)
            {
                string[] parameterComponents = parameterPair.Split('=', 2);
                if (parameterComponents.Length == 2 && parameterComponents[0] == targetParameter)
                {
                    return Uri.UnescapeDataString(parameterComponents[1]);
                }
            }
            throw new ArgumentException($"Parameter '{targetParameter}' not found in URL: {fullUrl}", nameof(fullUrl));
        }

        public (string codeVerifier, string codeChallenge) GeneratePkceData()
        {
            RandomNumberGenerator cryptographicRandomGenerator = RandomNumberGenerator.Create();
            byte[] randomVerifierBytes = new byte[32];
            cryptographicRandomGenerator.GetBytes(randomVerifierBytes);

            string codeVerifier = Convert.ToBase64String(randomVerifierBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            using (SHA256 sha256Hasher = SHA256.Create())
            {
                byte[] verifierHashBytes = sha256Hasher.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                string codeChallenge = Convert.ToBase64String(verifierHashBytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');

                return (codeVerifier, codeChallenge);
            }
        }

        public TwitterUserInfoResponse ExtractUserInfoFromIdToken(string jwtIdToken)
        {
            string[] tokenComponents = jwtIdToken.Split('.');
            if (tokenComponents.Length != 3)
            {
                throw new ArgumentException("Invalid ID token format.", nameof(jwtIdToken));
            }

            string base64UrlEncodedPayload = tokenComponents[1];
            while (base64UrlEncodedPayload.Length % 4 != 0)
            {
                base64UrlEncodedPayload += '=';
            }
            byte[] decodedPayloadBytes = Convert.FromBase64String(base64UrlEncodedPayload.Replace('-', '+').Replace('_', '/'));
            string decodedPayloadJson = Encoding.UTF8.GetString(decodedPayloadBytes);
            System.Text.Json.JsonSerializerOptions jsonDeserializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return System.Text.Json.JsonSerializer.Deserialize<TwitterUserInfoResponse>(decodedPayloadJson, jsonDeserializerOptions)
                ?? throw new Exception("Failed to deserialize ID token payload.");
        }
    }
    public class TwitterTokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public required int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public required string Scope { get; set; }
    }

    public class TwitterUserInfoResponse
    {
        [JsonPropertyName("data")]
        public required TwitterUserData Data { get; set; }
    }

    public class TwitterUserData
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("username")]
        public required string Username { get; set; }
    }
}