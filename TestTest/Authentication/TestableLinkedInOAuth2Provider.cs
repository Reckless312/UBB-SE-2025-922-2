using System;
using System.Net.Http;
using DrinkDb_Auth.AuthProviders.LinkedIn;
using DataAccess.Model;
using DrinkDb_Auth.OAuthProviders;
using IRepository;
using DataAccess.Model.Authentication;
using System.Text.Json;

namespace Tests.Authentication
{
    /// <summary>
    /// A testable version of LinkedInOAuth2Provider that allows injecting dependencies
    /// </summary>
    public class TestableLinkedInOAuth2Provider : LinkedInOAuth2Provider
    {
        private readonly HttpClient httpClient;
        private readonly IUserRepository userRepository;
        private readonly ISessionRepository sessionRepository;
        private IUserRepository object1;
        private ISessionRepository object2;

        public TestableLinkedInOAuth2Provider(HttpClient httpClient, IUserRepository object1, ISessionRepository object2)
        {
            this.httpClient = httpClient;
            this.object1 = object1;
            this.object2 = object2;
        }

        // Override the Authenticate method to use our mocked HttpClient instead of creating a new one
        public new AuthenticationResponse Authenticate(string userId, string token)
        {
            // Use the same implementation as the parent class but with our injected HttpClient
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "DrinkDb_Auth-App");
            
            try 
            {
                HttpResponseMessage response = httpClient.GetAsync("https://api.linkedin.com/v2/userinfo").Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Failed to fetch user info from Linkedin.");
                }

                string json = response.Content.ReadAsStringAsync().Result;
                return ProcessResponseJson(json, token);
            }
            catch (AggregateException exception) 
            {
                // Unwrap the inner exception if it's an HttpRequestException
                if (exception.InnerException is HttpRequestException httpException)
                {
                    throw httpException;
                }
                throw;
            }
        }

        // Helper method to process the response JSON
        private AuthenticationResponse ProcessResponseJson(string json, string token)
        {
            using (JsonDocument document = System.Text.Json.JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;
                
                // Safely get properties with null checking
                string id = "";
                string name = "";
                
                if (root.TryGetProperty("sub", out JsonElement subElement) && !subElement.ValueKind.Equals(System.Text.Json.JsonValueKind.Null))
                {
                    id = subElement.GetString() ?? "";
                }
                
                if (root.TryGetProperty("name", out JsonElement nameElement) && !nameElement.ValueKind.Equals(System.Text.Json.JsonValueKind.Null))
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
                if (userRepository == null)
                {
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = Guid.NewGuid(),
                        NewAccount = true
                    };
                }

                User user = userRepository.GetUserByUsername(name);
                if (user == null)
                {
                    // Create a new user since none exists
                    User newUser = new User
                    {
                        Username = name,
                        PasswordHash = string.Empty,
                        UserId = Guid.NewGuid(),
                        TwoFASecret = string.Empty,
                    };
                    
                    bool userCreated = true;
                    if (userRepository != null)
                    {
                        userCreated = userRepository.CreateUser(newUser);
                    }
                    
                    // Create a session for the new user
                    Guid sessionId = Guid.NewGuid();
                    if (sessionRepository != null && userCreated)
                    {
                        Session session = sessionRepository.CreateSession(newUser.UserId);
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
                    if (sessionRepository != null)
                    {
                        Session session = sessionRepository.CreateSession(user.UserId);
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