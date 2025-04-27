using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.Web.WebView2.Core;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.OAuthProviders;

namespace DrinkDb_Auth.AuthProviders.Twitter
{
    /// <summary>
    /// A PKCE-based OAuth 2.0 flow for Twitter in a WinUI desktop app.
    /// </summary>
    public class TwitterOAuth2Provider : GenericOAuth2Provider, ITwitterOAuth2Provider
    {
        private static readonly UserAdapter UserAdapter = new ();
        private static readonly SessionAdapter SessionAdapter = new ();
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

        public TwitterOAuth2Provider()
        {
            httpClient = new HttpClient();

            // Load from config (if you wish):
            ClientId = System.Configuration.ConfigurationManager.AppSettings["TwitterClientId"] ?? "YOUR_CLIENT_ID";
            ClientSecret = System.Configuration.ConfigurationManager.AppSettings["TwitterClientSecret"] ?? "YOUR_CLIENT_SECRET";

            System.Diagnostics.Debug.WriteLine($"Loaded Twitter ClientId: {ClientId}");
            System.Diagnostics.Debug.WriteLine($"Loaded Twitter ClientSecret: {ClientSecret.Substring(0, Math.Min(4, ClientSecret.Length))}... (not used in PKCE)");
        }

        /// <summary>
        /// Optional method to quickly verify a stored token (not used in this flow).
        /// </summary>
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

        /// <summary>
        /// Generates the full authorization URL with PKCE code challenge and needed query params.
        /// </summary>
        public string GetAuthorizationUrl()
        {
            // 2) PKCE: Generate a code_verifier & code_challenge
            var (generatedCodeVerifier, generatedCodeChallenge) = GeneratePkceData();
            this.codeVerifier = generatedCodeVerifier;  // store for later use in token request

            var concatenatedScopes = string.Join(" ", scopes);

            var authorizationParameters = new Dictionary<string, string>
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
            var encodedQueryString = string.Join("&", authorizationParameters
                .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            var fullAuthorizationUrl = $"{AuthorizationEndpoint}?{encodedQueryString}";
            System.Diagnostics.Debug.WriteLine($"Generated authorization URL: {fullAuthorizationUrl}");
            return fullAuthorizationUrl;
        }

        /// <summary>
        /// When we get the code back from Twitter, exchange it for an access token.
        /// PKCE: We do NOT pass a client_secret, but we DO pass the same code_verifier we generated earlier.
        /// </summary>
        public async Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code)
        {
            // 3) PKCE: Provide the stored code_verifier in the token request
            var tokenRequestParameters = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", ClientId },
                { "redirect_uri", RedirectUri },
                { "grant_type", "authorization_code" },
                { "code_verifier", codeVerifier }, // crucial for PKCE
            };

            System.Diagnostics.Debug.WriteLine("Exchanging code for token (PKCE).");
            foreach (var tokenParameter in tokenRequestParameters)
            {
                System.Diagnostics.Debug.WriteLine($"  {tokenParameter.Key}: {tokenParameter.Value}");
            }

            try
            {
                using var requestContent = new FormUrlEncodedContent(tokenRequestParameters);
                var tokenResponse = await httpClient.PostAsync(TokenEndpoint, requestContent);
                var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

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
                    using var twitterUserInfoClient = new HttpClient();
                    twitterUserInfoClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", twitterTokenResult.AccessToken);

                    var userInfoResponse = await twitterUserInfoClient.GetAsync(UserInfoEndpoint);
                    var userInfoResponseBody = await userInfoResponse.Content.ReadAsStringAsync();

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

                    var twitterUserInfoObject = System.Text.Json.JsonSerializer.Deserialize<TwitterUserInfoResponse>(userInfoResponseBody);
                    System.Diagnostics.Debug.WriteLine($"Authenticated user: {twitterUserInfoObject?.Data.Id} ({twitterUserInfoObject?.Data.Username})");
                    User? user = UserAdapter.GetUserByUsername(twitterUserInfoObject?.Data.Username ?? throw new Exception("user not found in json response payload for Twitter authentication"));
                    if (user == null)
                    {
                        // Create a new user
                        user = new User
                        {
                            Username = twitterUserInfoObject?.Data.Username ?? throw new Exception("user not found in json response payload for Twitter authentication"),
                            PasswordHash = string.Empty,
                            UserId = Guid.NewGuid(),
                            TwoFASecret = string.Empty,
                        };
                        UserAdapter.CreateUser(user);
                    }
                    else
                    {
                        // Update existing user if needed
                        UserAdapter.UpdateUser(user);
                    }

                    Session userSession = SessionAdapter.CreateSession(user.UserId);
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

        /// <summary>
        /// Shows a WebView, navigates to the Twitter OAuth page, intercepts the redirect to our local loopback.
        /// </summary>
        public async Task<AuthenticationResponse> SignInWithTwitterAsync(Window parentWindow)
        {
            var twitterAuthenticationCompletion = new TaskCompletionSource<AuthenticationResponse>();

            try
            {
                var twitterLoginDialog = new ContentDialog
                {
                    Title = "Sign in with Twitter",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = parentWindow.Content.XamlRoot
                };

                var twitterLoginWebView = new WebView2
                {
                    Width = 450,
                    Height = 600
                };
                twitterLoginDialog.Content = twitterLoginWebView;

                // Initialize the WebView2
                await twitterLoginWebView.EnsureCoreWebView2Async();

                // Listen for navigations
                twitterLoginWebView.CoreWebView2.NavigationStarting += async (sender, navigationArgs) =>
                {
                    var callbackUrl = navigationArgs.Uri;
                    System.Diagnostics.Debug.WriteLine($"NavigationStarting -> {callbackUrl}");

                    // If it's the redirect back to our loopback, we parse out the code
                    if (callbackUrl.StartsWith(RedirectUri, StringComparison.OrdinalIgnoreCase))
                    {
                        navigationArgs.Cancel = true; // don't actually navigate to 127.0.0.1 in the WebView

                        var receivedAuthCode = ExtractQueryParameter(callbackUrl, "code");
                        System.Diagnostics.Debug.WriteLine($"Found 'code' in callback: {receivedAuthCode}");

                        var twitterAuthResponse = await ExchangeCodeForTokenAsync(receivedAuthCode);

                        // Close the dialog and return
                        parentWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            twitterLoginDialog.Hide();
                            twitterAuthenticationCompletion.SetResult(twitterAuthResponse);
                        });
                    }
                };

                // Start the auth flow
                twitterLoginWebView.CoreWebView2.Navigate(GetAuthorizationUrl());

                // Display Twitter login dialog
                var dialogCompletionResult = await twitterLoginDialog.ShowAsync();

                // If user closed the dialog manually before we got a code
                if (!twitterAuthenticationCompletion.Task.IsCompleted)
                {
                    System.Diagnostics.Debug.WriteLine("Dialog closed; no code was returned.");
                    twitterAuthenticationCompletion.SetResult(new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    });
                }
            }
            catch (Exception twitterAuthenticationError)
            {
                System.Diagnostics.Debug.WriteLine($"SignInWithTwitterAsync error: {twitterAuthenticationError.Message}");
                twitterAuthenticationCompletion.TrySetException(twitterAuthenticationError);
            }

            return await twitterAuthenticationCompletion.Task;
        }

        /// <summary>
        /// Helper: parse one query param (e.g. ?code=xxx) from a URL
        /// </summary>
        private string ExtractQueryParameter(string fullUrl, string targetParameter)
        {
            var uriObject = new Uri(fullUrl);
            var queryString = uriObject.Query.TrimStart('?');
            var parameterPairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var parameterPair in parameterPairs)
            {
                var parameterComponents = parameterPair.Split('=', 2);
                if (parameterComponents.Length == 2 && parameterComponents[0] == targetParameter)
                {
                    return Uri.UnescapeDataString(parameterComponents[1]);
                }
            }
            throw new ArgumentException($"Parameter '{targetParameter}' not found in URL: {fullUrl}", nameof(fullUrl));
        }

        /// <summary>
        /// Generates PKCE (Proof Key for Code Exchange) security codes for OAuth 2.0.
        /// Creates a random code verifier and its corresponding SHA256-hashed challenge.
        /// Used to prevent authorization code interception attacks.
        /// </summary>
        /// <returns>
        /// A tuple containing:
        /// - codeVerifier: A random Base64URL-encoded string for verification
        /// - codeChallenge: SHA256 hash of the verifier, also Base64URL-encoded
        /// </returns>
        private (string codeVerifier, string codeChallenge) GeneratePkceData()
        {
            // code_verifier: a random 43–128 char string
            var cryptographicRandomGenerator = RandomNumberGenerator.Create();
            var randomVerifierBytes = new byte[32];
            cryptographicRandomGenerator.GetBytes(randomVerifierBytes);

            // Base64Url-encode without padding
            var codeVerifier = Convert.ToBase64String(randomVerifierBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            // code_challenge: SHA256 hash of verifier, then Base64Url-encode
            using (var sha256Hasher = SHA256.Create())
            {
                var verifierHashBytes = sha256Hasher.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                var codeChallenge = Convert.ToBase64String(verifierHashBytes)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');

                return (codeVerifier, codeChallenge);
            }
        }

        /// <summary>
        /// Extracts user information from a JWT (JSON Web Token) ID token if provided by Twitter.
        /// Note: Twitter typically doesn't provide ID tokens in their standard OAuth flow.
        /// </summary>
        /// <param name="jwtIdToken">The JWT format ID token from Twitter</param>
        /// <returns>Parsed user information from the token's payload</returns>
        /// <exception cref="ArgumentException">Thrown when the token format is invalid</exception>
        /// <exception cref="Exception">Thrown when payload deserialization fails</exception>
        private TwitterUserInfoResponse ExtractUserInfoFromIdToken(string jwtIdToken)
        {
            // Split JWT into header.payload.signature
            var tokenComponents = jwtIdToken.Split('.');
            if (tokenComponents.Length != 3)
            {
                throw new ArgumentException("Invalid ID token format.", nameof(jwtIdToken));
            }

            // Get the payload (middle part of JWT)
            var base64UrlEncodedPayload = tokenComponents[1];
            while (base64UrlEncodedPayload.Length % 4 != 0)
            {
                base64UrlEncodedPayload += '=';
            }
            // Convert from Base64URL to regular Base64 and decode
            var decodedPayloadBytes = Convert.FromBase64String(base64UrlEncodedPayload.Replace('-', '+').Replace('_', '/'));
            var decodedPayloadJson = Encoding.UTF8.GetString(decodedPayloadBytes);

            // Configure JSON deserialization options
            var jsonDeserializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Deserialize the JSON payload into our user info model
            return System.Text.Json.JsonSerializer.Deserialize<TwitterUserInfoResponse>(decodedPayloadJson, jsonDeserializerOptions)
                ?? throw new Exception("Failed to deserialize ID token payload.");
        }
    }

    /// <summary>
    /// Represents the OAuth 2.0 token response received from Twitter's authentication endpoint.
    /// </summary>
    /// <remarks>
    /// The response includes:
    /// - access_token: The token used to make API requests
    /// - token_type: Usually "Bearer"
    /// - expires_in: Token lifetime in seconds
    /// - refresh_token: Only present when "offline.access" scope is requested
    /// - scope: Space-separated list of granted scopes
    /// </remarks>
    public class TwitterTokenResponse
    {
        /// <summary>
        /// Gets or sets the access token used for API requests.
        /// </summary>
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the type of token, typically "Bearer".
        /// </summary>
        [JsonPropertyName("token_type")]
        public required string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the token lifetime in seconds.
        /// </summary>
        [JsonPropertyName("expires_in")]
        public required int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the refresh token for obtaining new access tokens.
        /// Only present when "offline.access" scope was requested.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the space-separated list of granted scopes.
        /// </summary>
        [JsonPropertyName("scope")]
        public required string Scope { get; set; }
    }

    /// <summary>
    /// Represents the user information response from Twitter's /2/users/me endpoint.
    /// </summary>
    /// <remarks>
    /// The response contains user data in a nested structure.
    /// Some fields like email require specific permissions and scopes.
    /// </remarks>
    public class TwitterUserInfoResponse
    {
        /// <summary>
        /// Gets or sets the user data contained in the response.
        /// </summary>
        [JsonPropertyName("data")]
        public required TwitterUserData Data { get; set; }
    }

    /// <summary>
    /// Represents the core user data fields returned by Twitter.
    /// </summary>
    public class TwitterUserData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the Twitter user.
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the Twitter user.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the username/handle of the Twitter user (without @ symbol).
        /// </summary>
        [JsonPropertyName("username")]
        public required string Username { get; set; }
    }
}