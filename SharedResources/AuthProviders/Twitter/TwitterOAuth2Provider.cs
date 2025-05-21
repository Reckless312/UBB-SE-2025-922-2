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
    using IRepository;

    /// <summary>
    /// A PKCE-based OAuth 2.0 flow for Twitter in a WinUI desktop app.
    /// </summary>
    public class TwitterOAuth2Provider : ITwitterOAuth2Provider
    {
        private IUserRepository UserRepository;
        private ISessionRepository SessionAdapter;
        // ▼▼▼ 1) Set these appropriately ▼▼▼

        // In "Native App" flows, we typically do NOT use a Client Secret.
        // but if you still have one in your config, you can read it; just don't send it.
        private string ClientId { get; }
        private string ClientSecret { get; } // not used if truly "native"

        // The same Callback/Redirect URI you registered in Twitter Developer Portal.
        // e.g. "http://127.0.0.1:5000/x-callback"
        private const string RedirectUri = "http://127.0.0.1:5000/x-callback";

        // Twitter endpoints:
        private const string AuthorizationEndpoint = "https://twitter.com/i/oauth2/authorize";
        private const string TokenEndpoint = "https://api.twitter.com/2/oauth2/token";
        private const string UserInfoEndpoint = "https://api.twitter.com/2/users/me";

        // Example scopes. If you want refresh tokens, include "offline.access".
        private readonly string[] scopes = { "tweet.read", "users.read" };

        // Private fields for PKCE
        private string codeVerifier = string.Empty;

        private readonly HttpClient httpClient;

        public TwitterOAuth2Provider(IUserRepository UserRepository, ISessionRepository SessionAdapter)
        {
            httpClient = new HttpClient();

            // Load from config (if you wish):
            ClientId = "ODVNN2VYRGR4ZWNfcm9LQnlzS2Q6MTpjaQ";
            ClientSecret = "B7eMoprWDmTGzsYz - 3kK8Hsqpc5oJ4i4Gt9tjqtFb73J5dBQyz";

            System.Diagnostics.Debug.WriteLine($"Loaded Twitter ClientId: {ClientId}");
            System.Diagnostics.Debug.WriteLine($"Loaded Twitter ClientSecret: {ClientSecret.Substring(0, Math.Min(4, ClientSecret.Length))}... (not used in PKCE)");

            this.UserRepository = UserRepository;
            this.SessionAdapter = SessionAdapter;
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
            // 2) PKCE: Generate a code_verifier & code_challenge
            (string generatedCodeVerifier, string generatedCodeChallenge) = GeneratePkceData();
            this.codeVerifier = generatedCodeVerifier;  // store for later use in token request

            string concatenatedScopes = string.Join(" ", scopes);

            Dictionary<string, string> authorizationParameters = new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "redirect_uri", RedirectUri },
                { "response_type", "code" },
                { "scope", concatenatedScopes },
                { "state", Guid.NewGuid().ToString() },

                // PKCE
                { "code_challenge", generatedCodeChallenge },
                { "code_challenge_method", "S256" }
            };

            // Build the query string
            string encodedQueryString = string.Join("&", authorizationParameters
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            string fullAuthorizationUrl = $"{AuthorizationEndpoint}?{encodedQueryString}";
            System.Diagnostics.Debug.WriteLine($"Generated authorization URL: {fullAuthorizationUrl}");
            return fullAuthorizationUrl;
        }

        public async Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code)
        {
            // 3) PKCE: Provide the stored code_verifier in the token request
            Dictionary<string, string> tokenRequestParameters = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", ClientId },
                { "redirect_uri", RedirectUri },
                { "grant_type", "authorization_code" },
                { "code_verifier", codeVerifier }, // crucial for PKCE
            };

            System.Diagnostics.Debug.WriteLine("Exchanging code for token (PKCE).");
            foreach (KeyValuePair<string, string> tokenParameter in tokenRequestParameters)
            {
                System.Diagnostics.Debug.WriteLine($"  {tokenParameter.Key}: {tokenParameter.Value}");
            }

            try
            {
                using FormUrlEncodedContent requestContent = new FormUrlEncodedContent(tokenRequestParameters);
                HttpResponseMessage tokenResponse = await httpClient.PostAsync(TokenEndpoint, requestContent);
                string tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"Token Response status: {tokenResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Token Response content: {tokenResponseContent}");

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("Token request failed with non-success status.");
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    };
                }

                // Deserialize token response
                TwitterTokenResponse? twitterTokenResult;
                try
                {
                    twitterTokenResult = await tokenResponse.Content.ReadFromJsonAsync<TwitterTokenResponse>();
                }
                catch
                {
                    // fallback if ReadFromJsonAsync fails
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

                // 4) Optionally, get user info
                try
                {
                    using HttpClient twitterUserInfoClient = new HttpClient();
                    twitterUserInfoClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", twitterTokenResult.AccessToken);

                    HttpResponseMessage userInfoResponse = await twitterUserInfoClient.GetAsync(UserInfoEndpoint);
                    string userInfoResponseBody = await userInfoResponse.Content.ReadAsStringAsync();

                    if (!userInfoResponse.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"User info request failed. Response: {userInfoResponseBody}");
                        // We still have a valid token though
                        return new AuthenticationResponse
                        {
                            AuthenticationSuccessful = false,
                            OAuthToken = twitterTokenResult.AccessToken,
                            SessionId = Guid.Empty,
                            NewAccount = false
                        };
                    }

                    TwitterUserInfoResponse? twitterUserInfoObject = System.Text.Json.JsonSerializer.Deserialize<TwitterUserInfoResponse>(userInfoResponseBody);
                    System.Diagnostics.Debug.WriteLine($"Authenticated user: {twitterUserInfoObject?.Data.Id} ({twitterUserInfoObject?.Data.Username})");
                    User? user = UserRepository.GetUserByUsername(twitterUserInfoObject?.Data.Username ?? throw new Exception("user not found in json response payload for Twitter authentication")).Result;
                    if (user == null)
                    {
                        // Create a new user
                        user = new User
                        {
                            Username = twitterUserInfoObject?.Data.Username,
                            PasswordHash = string.Empty,
                            UserId = Guid.NewGuid(),
                            TwoFASecret = string.Empty,
                            EmailAddress = "cevaemail",
                            NumberOfDeletedReviews = 0,
                            HasSubmittedAppeal = false,
                            AssignedRole = RoleType.Admin,
                            FullName = twitterUserInfoObject?.Data.Username,
                        };
                        await UserRepository.CreateUser(user);
                    }
                    else
                    {
                        // Update existing user if needed
                        await UserRepository.UpdateUser(user);
                    }

                    Session userSession = SessionAdapter.CreateSession(user.UserId).Result;
                    return new AuthenticationResponse
                    {
                        OAuthToken = twitterTokenResult.AccessToken,
                        AuthenticationSuccessful = true,
                        SessionId = userSession.SessionId,
                        NewAccount = false
                    };
                }
                catch (Exception userInfoException)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception fetching user info: {userInfoException.Message}");
                    // We'll still consider the token valid
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    };
                }
            }
            catch (Exception tokenExchangeException)
            {
                System.Diagnostics.Debug.WriteLine($"ExchangeCodeForTokenAsync exception: {tokenExchangeException.Message}");
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
            // code_verifier: a random 43–128 char string
            RandomNumberGenerator cryptographicRandomGenerator = RandomNumberGenerator.Create();
            byte[] randomVerifierBytes = new byte[32];
            cryptographicRandomGenerator.GetBytes(randomVerifierBytes);

            // Base64Url-encode without padding
            string codeVerifier = Convert.ToBase64String(randomVerifierBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            // code_challenge: SHA256 hash of verifier, then Base64Url-encode
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
            // Split JWT into header.payload.signature
            string[] tokenComponents = jwtIdToken.Split('.');
            if (tokenComponents.Length != 3)
            {
                throw new ArgumentException("Invalid ID token format.", nameof(jwtIdToken));
            }

            // Get the payload (middle part of JWT)
            string base64UrlEncodedPayload = tokenComponents[1];
            while (base64UrlEncodedPayload.Length % 4 != 0)
            {
                base64UrlEncodedPayload += '=';
            }
            // Convert from Base64URL to regular Base64 and decode
            byte[] decodedPayloadBytes = Convert.FromBase64String(base64UrlEncodedPayload.Replace('-', '+').Replace('_', '/'));
            string decodedPayloadJson = Encoding.UTF8.GetString(decodedPayloadBytes);

            // Configure JSON deserialization options
            System.Text.Json.JsonSerializerOptions jsonDeserializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Deserialize the JSON payload into our user info model
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