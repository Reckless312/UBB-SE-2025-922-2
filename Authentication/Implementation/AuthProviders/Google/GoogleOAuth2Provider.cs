using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DrinkDb_Auth.Adapter;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DrinkDb_Auth.Model;
using Microsoft.AspNetCore.Http;
using DrinkDb_Auth.OAuthProviders;

namespace DrinkDb_Auth.AuthProviders.Google
{
    public class GoogleOAuth2Provider : GenericOAuth2Provider, IGoogleOAuth2Provider
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
        private static readonly ISessionAdapter SessionDatabaseAdapter = new SessionAdapter();
        private static readonly IUserAdapter UserDatabaseAdapter = new UserAdapter();
        private Guid EnsureUserExists(string identifier, string email, string name)
        {
            Guid userId = CreateGloballyUniqueIdentifier(identifier);
            User? user = UserDatabaseAdapter.GetUserById(userId);

            switch (user)
            {
                case null:
                    // Don't know why email is used as username but let's vibe with it
                    User newUser = new User { UserId = userId, Username = email, PasswordHash = string.Empty, TwoFASecret = null };
                    bool wasCreated = UserDatabaseAdapter.CreateUser(newUser);
                    break;
                case not null:
                    break;
            }

            return userId;
        }

        public GoogleOAuth2Provider()
        {
            System.Collections.Specialized.NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
            httpClient = new HttpClient();

            ClientId = "311954949107-k5agbsvuvrsuttupcu7av2lceuk4vlag.apps.googleusercontent.com";
            ClientSecret = "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";
            RedirectUniformResourceIdentifier = "urn:ietf:wg:oauth:2.0:oob";
            AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
            TokenEndpoint = "https://oauth2.googleapis.com/token";
            UserInformationEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        }

        public AuthenticationResponse Authenticate(string userId, string token)
        {
            return new AuthenticationResponse { AuthenticationSuccessful = !string.IsNullOrEmpty(token), OAuthToken = token, SessionId = Guid.Empty, NewAccount = false };
        }

        public string GetAuthorizationUrl()
        {
            string allowedResourcesScope = string.Join(" ", userResourcesScope);

            Dictionary<string, string> authorizationData = new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "redirect_uri", RedirectUniformResourceIdentifier },
                { "response_type", "code" },
                { "scope", allowedResourcesScope },
                { "access_type", "offline" },
                { "state", Guid.NewGuid().ToString() }
            };

            string transformedURLData = string.Join("&", authorizationData.Select(row => $"{Uri.EscapeDataString(row.Key)}={Uri.EscapeDataString(row.Value)}"));
            string fullAuthorizationURL = $"{AuthorizationEndpoint}?{transformedURLData}";

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
                                        userId = EnsureUserExists(userInformation.Identifier, httpClientInformation.Email, httpClientInformation.Name);
                                        return new AuthenticationResponse { AuthenticationSuccessful = true, OAuthToken = tokenResult.AccessToken, SessionId = SessionDatabaseAdapter.CreateSession(userId).SessionId, NewAccount = false };
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
                            userId = EnsureUserExists(userInformation.Identifier, userInformation.Email, userInformation.Name);
                            return new AuthenticationResponse { AuthenticationSuccessful = true, OAuthToken = tokenResult.AccessToken, SessionId = SessionDatabaseAdapter.CreateSession(userId).SessionId, NewAccount = false };
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

        public async Task<AuthenticationResponse> SignInWithGoogleAsync(Window parentWindow)
        {
            TaskCompletionSource<AuthenticationResponse> taskResults = new TaskCompletionSource<AuthenticationResponse>();
            try
            {
                ContentDialog googleSubWindow = new ContentDialog
                {
                    Title = "Sign in with Google",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = parentWindow.Content.XamlRoot
                };

                WebView2 webView = new WebView2();
                webView.Width = 450;
                webView.Height = 600;
                googleSubWindow.Content = webView;

                await webView.EnsureCoreWebView2Async();
                bool authenticationCodeFound = false;
                string approvalPath = "accounts.google.com/o/oauth2/approval";
                string domTitle = "document.title";
                string domBodyText = "document.body.innerText";
                string javaScriptQuery = "document.querySelector('code') ? document.querySelector('code').innerText : ''";
                string pageSuccesContent = "Succes", pageCodeContent = "code", pageSuccesCode = "Success code=";
                string pageCodeContentWithEqualAtTheEnd = pageCodeContent + "=";
                webView.CoreWebView2.DOMContentLoaded += async (sender, args) =>
                {
                    try
                    {
                        string currentUrl = webView.CoreWebView2.Source;
                        string title = await webView.CoreWebView2.ExecuteScriptAsync(domTitle);
                        string javaScriptCode = @"(function() 
                                                {
                                                    const codeElement = document.querySelector('textarea.kHn9Lb');

                                                    if (codeElement) return codeElement.textContent;
                                    
                                                    const possibleCodeElements = document.querySelectorAll('code, pre, textarea, input[readonly]');

                                                    for (const el of possibleCodeElements)
                                                    {
                                                        const content = el.textContent || el.value;
                                                        if (content && content.length > 10) return content;
                                                    }
                                    
                                                    return '';
                                                })()";

                        switch (currentUrl.Contains(approvalPath))
                        {
                            case true:
                                string functionResult = await webView.CoreWebView2.ExecuteScriptAsync(javaScriptCode);
                                string trimmedResult = functionResult.Trim('"');

                                if (!string.IsNullOrEmpty(trimmedResult) && trimmedResult != "null" && !authenticationCodeFound)
                                {
                                    authenticationCodeFound = true;
                                    AuthenticationResponse authenticationResponse = await ExchangeCodeForTokenAsync(trimmedResult);
                                    parentWindow.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        try
                                        {
                                            googleSubWindow.Hide();
                                        }
                                        catch
                                        {
                                        }
                                        taskResults.SetResult(authenticationResponse);
                                    });

                                    return;
                                }
                                break;
                            case false:
                                break;
                        }

                        string code = string.Empty;
                        if (title.Contains(pageSuccesContent) || title.Contains(pageCodeContent))
                        {
                            switch (title.Contains(pageSuccesCode))
                            {
                                case true:
                                    title = title.Replace("\"", string.Empty);
                                    code = title.Substring(title.IndexOf("Success code=") + "Success code=".Length);
                                    break;
                            }

                            switch (string.IsNullOrEmpty(code))
                            {
                                case true:
                                    string pageContent = await webView.CoreWebView2.ExecuteScriptAsync(domBodyText);
                                    if (pageContent.Contains(pageCodeContentWithEqualAtTheEnd))
                                    {
                                        int skipNumberOfElements = 5;
                                        int startIndex = pageContent.IndexOf(pageCodeContentWithEqualAtTheEnd) + skipNumberOfElements;
                                        int endIndex = pageContent.IndexOf("\"", startIndex);

                                        code = endIndex > startIndex ? code = pageContent.Substring(startIndex, endIndex - startIndex) : code;
                                    }

                                    string codeElement = await webView.CoreWebView2.ExecuteScriptAsync(javaScriptQuery);

                                    code = !string.IsNullOrEmpty(codeElement) && codeElement != "\"\"" ? codeElement.Trim('"') : code;
                                    break;
                            }

                            switch (!string.IsNullOrEmpty(code) && !authenticationCodeFound)
                            {
                                case true:
                                    authenticationCodeFound = true;
                                    AuthenticationResponse response = await ExchangeCodeForTokenAsync(code);
                                    parentWindow.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        try
                                        {
                                            googleSubWindow.Hide();
                                        }
                                        catch
                                        {
                                        }
                                        taskResults.SetResult(response);
                                    });
                                    break;
                            }
                        }
                    }
                    catch
                    {
                    }
                };

                webView.NavigationCompleted += async (sender, args) =>
                {
                    try
                    {
                        string uniformResourceIdentifier = webView.Source?.ToString() ?? webView.CoreWebView2.Source;
                        if (uniformResourceIdentifier.Contains(approvalPath) && !authenticationCodeFound)
                        {
                            await Task.Delay(500);

                            string code = string.Empty;

                            string pageContent = await webView.CoreWebView2.ExecuteScriptAsync(domBodyText);

                            switch (pageContent.Contains(pageCodeContentWithEqualAtTheEnd))
                            {
                                case true:
                                    int skipNumberOfElements = 5;
                                    int startIndex = pageContent.IndexOf(pageCodeContentWithEqualAtTheEnd) + skipNumberOfElements;
                                    int endIndex = pageContent.IndexOf(" ", startIndex);

                                    code = endIndex > startIndex ? code = pageContent.Substring(startIndex, endIndex - startIndex).Replace("\"", string.Empty).Trim() : code;
                                    break;
                            }

                            string javaScriptArrayStream = "Array.from(document.querySelectorAll('code, .auth-code, input[readonly]')).map(el => el.innerText || el.value)";
                            string codeElements = await webView.CoreWebView2.ExecuteScriptAsync(javaScriptArrayStream);

                            switch (codeElements != "[]" && !string.IsNullOrEmpty(codeElements))
                            {
                                case true:
                                    string[] elements = codeElements.Trim('[', ']').Split(',');
                                    foreach (string element in elements)
                                    {
                                        string trimmedValue = element.Trim('"', ' ');
                                        if (!string.IsNullOrEmpty(trimmedValue) && trimmedValue.Length > 10)
                                        {
                                            code = trimmedValue;
                                            break;
                                        }
                                    }
                                    break;
                            }

                            switch (!string.IsNullOrEmpty(code) && !authenticationCodeFound)
                            {
                                case true:
                                    authenticationCodeFound = true;
                                    AuthenticationResponse response = await ExchangeCodeForTokenAsync(code);

                                    parentWindow.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        try
                                        {
                                            googleSubWindow.Hide();
                                        }
                                        catch
                                        {
                                        }
                                        taskResults.SetResult(response);
                                    });
                                    break;
                            }
                            }
                        }
                    catch
                    {
                    }
                };

                string authorizationURL = GetAuthorizationUrl();
                webView.CoreWebView2.Navigate(authorizationURL);
                ContentDialogResult subWindowResults = await googleSubWindow.ShowAsync();

                if (!taskResults.Task.IsCompleted)
                {
                    taskResults.SetResult(new AuthenticationResponse { AuthenticationSuccessful = false, OAuthToken = string.Empty, SessionId = Guid.Empty, NewAccount = false });
                }
            }
            catch (Exception ex)
            {
                taskResults.TrySetException(ex);
            }

            return await taskResults.Task;
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