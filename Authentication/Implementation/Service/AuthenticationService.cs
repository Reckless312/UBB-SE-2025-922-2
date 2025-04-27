using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
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

namespace DrinkDb_Auth.Service
{
    public enum OAuthService
    {
        Google,
        Facebook,
        Twitter,
        GitHub,
        LinkedIn
    }

    public class AuthenticationService : IAuthenticationService
    {
        private ISessionAdapter sessionAdapter;
        private IUserAdapter userAdapter;
        private ILinkedInLocalOAuthServer linkedinLocalServer;
        private IGitHubLocalOAuthServer githubLocalServer;
        private IFacebookLocalOAuthServer facebookLocalServer;
        private IBasicAuthenticationProvider basicAuthenticationProvider;

        public AuthenticationService()
        {
            githubLocalServer = new GitHubLocalOAuthServer("http://localhost:8890/");
            _ = githubLocalServer.StartAsync();

            facebookLocalServer = new FacebookLocalOAuthServer("http://localhost:8888/");
            _ = facebookLocalServer.StartAsync();

            linkedinLocalServer = new LinkedInLocalOAuthServer("http://localhost:8891/");
            _ = linkedinLocalServer.StartAsync();

            sessionAdapter = new SessionAdapter();

            basicAuthenticationProvider = new BasicAuthenticationProvider();

            userAdapter = new UserAdapter();
        }

        public AuthenticationService(ILinkedInLocalOAuthServer linkedinLocalServer, IGitHubLocalOAuthServer githubLocalServer, IFacebookLocalOAuthServer facebookLocalServer, IUserAdapter userAdapter, ISessionAdapter sessionAdapter, IBasicAuthenticationProvider basicAuthenticationProvider)
        {
            this.linkedinLocalServer = linkedinLocalServer;
            this.githubLocalServer = githubLocalServer;
            this.facebookLocalServer = facebookLocalServer;
            this.userAdapter = userAdapter;
            this.sessionAdapter = sessionAdapter;
            this.basicAuthenticationProvider = basicAuthenticationProvider;
            _ = githubLocalServer.StartAsync();
            _ = facebookLocalServer.StartAsync();
            _ = linkedinLocalServer.StartAsync();
        }

        public async Task<AuthenticationResponse> AuthWithOAuth(Window window, OAuthService selectedService, object authProvider)
        {
            var authResponse = selectedService switch
            {
                OAuthService.Google => await AuthenticateWithGoogleAsync(window, authProvider as IGoogleOAuth2Provider),
                OAuthService.Facebook => await AuthenticateWithFacebookAsync(authProvider as IFacebookOAuthHelper),
                OAuthService.Twitter => await AuthenticateWithTwitterAsync(window, authProvider as ITwitterOAuth2Provider),
                OAuthService.GitHub => await AuthenticateWithGitHubAsync(authProvider as IGitHubOAuthHelper),
                OAuthService.LinkedIn => await AuthenticateWithLinkedInAsync(authProvider as ILinkedInOAuthHelper),
                _ => throw new ArgumentException("Invalid OAuth service selected"),
            };

            if (authResponse.AuthenticationSuccessful)
            {
                App.CurrentSessionId = authResponse.SessionId;
                Session session = sessionAdapter.GetSession(App.CurrentSessionId);
                App.CurrentUserId = session.UserId;
            }

            return authResponse;
        }

        public virtual void Logout()
        {
            sessionAdapter.EndSession(App.CurrentSessionId);
            App.CurrentSessionId = Guid.Empty;
            App.CurrentUserId = Guid.Empty;
        }

        public virtual User GetUser(Guid sessionId)
        {
            Session session = sessionAdapter.GetSession(sessionId);
            return userAdapter.GetUserById(session.UserId) ?? throw new UserNotFoundException("User not found");
        }

        public AuthenticationResponse AuthWithUserPass(string username, string password)
        {
            try
            {
                if (basicAuthenticationProvider.Authenticate(username, password))
                {
                    User user = userAdapter.GetUserByUsername(username) ?? throw new UserNotFoundException("User not found");
                    Session session = sessionAdapter.CreateSession(user.UserId);
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        NewAccount = false,
                        OAuthToken = string.Empty,
                        SessionId = session.SessionId,
                    };
                }
                else
                {
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        NewAccount = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                    };
                }
            }
            catch (UserNotFoundException)
            {
                // create user
                User user = new ()
                {
                    Username = username,
                    PasswordHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)) ?? throw new Exception("Hashing failed")),
                    UserId = Guid.NewGuid(),
                    TwoFASecret = string.Empty
                };
                userAdapter.CreateUser(user);
                Session session = sessionAdapter.CreateSession(user.UserId);
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = true,
                    NewAccount = true,
                    OAuthToken = string.Empty,
                    SessionId = session.SessionId,
                };
            }
            throw new Exception("Unexpected error during authentication");
        }

        private static async Task<AuthenticationResponse> AuthenticateWithGitHubAsync(IGitHubOAuthHelper gitHubHelper)
        {
            return await gitHubHelper.AuthenticateAsync();
        }

        private static async Task<AuthenticationResponse> AuthenticateWithGoogleAsync(Window window, IGoogleOAuth2Provider googleProvider)
        {
            return await googleProvider.SignInWithGoogleAsync(window);
        }

        private static async Task<AuthenticationResponse> AuthenticateWithFacebookAsync(IFacebookOAuthHelper faceBookHelper)
        {
            return await faceBookHelper.AuthenticateAsync();
        }

        private static async Task<AuthenticationResponse> AuthenticateWithTwitterAsync(Window window, ITwitterOAuth2Provider twitterProvider)
        {
            return await twitterProvider.SignInWithTwitterAsync(window);
        }

        private static async Task<AuthenticationResponse> AuthenticateWithLinkedInAsync(ILinkedInOAuthHelper linkedInHelper)
        {
            return await linkedInHelper.AuthenticateAsync();
        }
    }
}
