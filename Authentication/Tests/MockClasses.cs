using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.AuthProviders;
using DrinkDb_Auth.AuthProviders.Facebook;
using DrinkDb_Auth.AuthProviders.Github;
using DrinkDb_Auth.AuthProviders.Google;
using DrinkDb_Auth.AuthProviders.LinkedIn;
using DrinkDb_Auth.AuthProviders.Twitter;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.OAuthProviders;
using Microsoft.UI.Xaml;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Tests
{
    public class MockSessionAdapter : ISessionAdapter
    {
        public Guid MockId { get; set; }

        public Session CreateSession(Guid userId)
        {
            return Session.CreateSessionWithIds(MockId, MockId);
        }

        public bool EndSession(Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public Session GetSession(Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public Session GetSessionByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }
    }

    public class MockGoogleAuthProvider : IGoogleOAuth2Provider
    {
        public Guid MockId { get; set; }

        public AuthenticationResponse Authenticate(string userId, string token)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code)
        {
            throw new NotImplementedException();
        }

        public string GetAuthorizationUrl()
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResponse> SignInWithGoogleAsync(Window parentWindow)
        {
            AuthenticationResponse mockResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = MockId,
            };

            return Task.FromResult(mockResponse);
        }
    }

    public class MockUserAdapter : IUserAdapter
    {
        public bool Throws { get; set; } = false;
        public Guid MockId { get; set; }
        public string MockUsername { get; set; } = "";
        public string MockPassword { get; set; } = "";
        public string? MockTwoFASecret { get; set; } = null;

        public bool CreateUser(User user)
        {
            if (Throws)
                throw new Exception();
            return true;
        }

        public User? GetUserById(Guid userId)
        {
            return userId == MockId ? new User { PasswordHash = MockPassword, TwoFASecret = MockTwoFASecret, UserId = MockId, Username = MockUsername } : null;
        }

        public User? GetUserByUsername(string username)
        {
            return username == MockUsername ? new User { PasswordHash = MockPassword, TwoFASecret = MockTwoFASecret, UserId = MockId, Username = MockUsername } : null;
        }

        public bool UpdateUser(User user)
        {
            return true;
        }

        public bool ValidateAction(Guid userId, string resource, string action)
        {
            return true;
        }
    }

    public class MockLinkedInServer : ILinkedInLocalOAuthServer
    {
        public static event Action<string>? OnCodeReceived;

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public void Stop()
        {
            /* does nothing */
        }
    }

    public class MockGitHubServer : IGitHubLocalOAuthServer
    {
        public static event Action<string>? OnCodeReceived;

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public void Stop()
        {
            /* does nothing */
        }
    }

    public class MockFacebookServer : IFacebookLocalOAuthServer
    {
        public static event Action<string>? OnTokenReceived;

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public void Stop()
        {
            /* does nothing */
        }
    }

    public class MockFacebookAuthProvider : IFacebookOAuthHelper
    {
        public Guid MockId { get; set; }
        public Task<AuthenticationResponse> AuthenticateAsync()
        {
            AuthenticationResponse mockResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = MockId,
            };

            return Task.FromResult(mockResponse);
        }
    }

    public class MockBasicAuth : IBasicAuthenticationProvider
    {
        public bool Succeeds { get; set; }
        public bool Authenticate(string username, string password)
        {
            return Succeeds;
        }
    }

    public class MockTwitterAuthProvider : ITwitterOAuth2Provider
    {
        public Guid MockId { get; set; }
        public Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code)
        {
            throw new NotImplementedException();
        }

        public string GetAuthorizationUrl()
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticationResponse> SignInWithTwitterAsync(Window parentWindow)
        {
            AuthenticationResponse mockResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = MockId,
            };

            return Task.FromResult(mockResponse);
        }
    }

    public class MockGitHubAuthProvider : IGitHubOAuthHelper
    {
        public Guid MockId { get; set;}
        public Task<AuthenticationResponse> AuthenticateAsync()
        {
            AuthenticationResponse mockResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = MockId,
            };

            return Task.FromResult(mockResponse);
        }
    }

    public class MockLinkedInAuthProvider : ILinkedInOAuthHelper
    {
        public Guid MockId { get; set;}
        public Task<AuthenticationResponse> AuthenticateAsync()
        {
            AuthenticationResponse mockResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = MockId,
            };

            return Task.FromResult(mockResponse);
        }
    }

    public class MockGitHubHttpHelper : IGitHubHttpHelper
    {
        public bool Throws { get; set; } = false;
        public string MockGitHubId { get; set;}
        public string MockGitHubLogin { get; set;}

        public bool IsListening { get; set; }

        public HttpListenerPrefixCollection Prefixes { get; set; }

        public (string gitHubId, string gitHubLogin) FetchGitHubUserInfo(string token)
        {
            if (Throws)
                throw new Exception();
            return (MockGitHubId, MockGitHubLogin);
        }

        public Task<HttpListenerContext> GetContextAsync()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
