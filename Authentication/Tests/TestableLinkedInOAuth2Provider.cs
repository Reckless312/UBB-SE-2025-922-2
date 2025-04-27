using System;
using System.Net.Http;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.AuthProviders.LinkedIn;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.OAuthProviders;

namespace DrinkDb_Auth.Tests
{
    /// <summary>
    /// A testable version of LinkedInOAuth2Provider that allows injecting dependencies
    /// </summary>
    public class TestableLinkedInOAuth2Provider : LinkedInOAuth2Provider
    {
        private readonly HttpClient _httpClient;
        private readonly IUserAdapter _userAdapter;
        private readonly ISessionAdapter _sessionAdapter;

        public TestableLinkedInOAuth2Provider(
            HttpClient httpClient, 
            IUserAdapter userAdapter = null, 
            ISessionAdapter sessionAdapter = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _userAdapter = userAdapter;
            _sessionAdapter = sessionAdapter;
        }

        // Override the Authenticate method to use our mocked HttpClient instead of creating a new one
        public new AuthenticationResponse Authenticate(string userId, string token)
        {
            // Use the same implementation as the parent class but with our injected HttpClient
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");
            
            try 
            {
                var response = _httpClient.GetAsync("https://api.linkedin.com/v2/userinfo").Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Failed to fetch user info from Linkedin.");
                }

                string json = response.Content.ReadAsStringAsync().Result;
                return ProcessResponseJson(json, token);
            }
            catch (AggregateException ex) 
            {
                // Unwrap the inner exception if it's an HttpRequestException
                if (ex.InnerException is HttpRequestException httpEx)
                {
                    throw httpEx;
                }
                throw;
            }
        }

        // Helper method to process the response JSON
        private AuthenticationResponse ProcessResponseJson(string json, string token)
        {
            using (var document = System.Text.Json.JsonDocument.Parse(json))
            {
                var root = document.RootElement;
                
                // Safely get properties with null checking
                string id = "";
                string name = "";
                
                if (root.TryGetProperty("sub", out var subElement) && !subElement.ValueKind.Equals(System.Text.Json.JsonValueKind.Null))
                {
                    id = subElement.GetString() ?? "";
                }
                
                if (root.TryGetProperty("name", out var nameElement) && !nameElement.ValueKind.Equals(System.Text.Json.JsonValueKind.Null))
                {
                    name = nameElement.GetString() ?? "";
                }

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                {
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    };
                }

                // If userAdapter is null, return a fake response
                if (_userAdapter == null)
                {
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = Guid.NewGuid(),
                        NewAccount = true
                    };
                }

                var user = _userAdapter.GetUserByUsername(name);
                if (user == null)
                {
                    // Create a new user since none exists
                    var newUser = new User
                    {
                        Username = name,
                        PasswordHash = string.Empty,
                        UserId = Guid.NewGuid(),
                        TwoFASecret = string.Empty,
                    };
                    
                    bool userCreated = true;
                    if (_userAdapter != null)
                    {
                        userCreated = _userAdapter.CreateUser(newUser);
                    }
                    
                    // Create a session for the new user
                    Guid sessionId = Guid.NewGuid();
                    if (_sessionAdapter != null && userCreated)
                    {
                        var session = _sessionAdapter.CreateSession(newUser.UserId);
                        if (session != null)
                        {
                            sessionId = session.SessionId;
                        }
                    }
                    
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = sessionId,
                        NewAccount = true
                    };
                }
                else
                {
                    // User exists, create a session for them
                    Guid sessionId = Guid.NewGuid();
                    if (_sessionAdapter != null)
                    {
                        var session = _sessionAdapter.CreateSession(user.UserId);
                        if (session != null)
                        {
                            sessionId = session.SessionId;
                        }
                    }
                    
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = sessionId,
                        NewAccount = false
                    };
                }
            }
        }
    }
} 