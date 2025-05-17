using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataAccess.AuthProviders;
using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.Github;
//using DataAccess.AuthProviders.Google;
using DataAccess.AuthProviders.LinkedIn;
//using DataAccess.AuthProviders.Twitter;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using IRepository;
using DataAccess.Service.Authentication.Interfaces;
using Repository.Authentication;
using Repository.AdminDashboard;
using Org.BouncyCastle.Tls;
using System.Collections.Generic;
using DataAccess.Model.AdminDashboard;
using ServerAPI.Data;

namespace DataAccess.Service.Authentication
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
        private ISessionRepository sessionRepository;
        private IUserRepository userRepository;
        private ILinkedInLocalOAuthServer linkedinLocalServer;
        private IGitHubLocalOAuthServer githubLocalServer;
        private IFacebookLocalOAuthServer facebookLocalServer;
        private IBasicAuthenticationProvider basicAuthenticationProvider;
        private static Guid currentSessionId = Guid.Empty;
        private static Guid currentUserId = Guid.Empty;

        public AuthenticationService(DatabaseContext context)
        {
            githubLocalServer = new GitHubLocalOAuthServer("http://localhost:8890/");
            _ = githubLocalServer.StartAsync();

            facebookLocalServer = new FacebookLocalOAuthServer("http://localhost:8888/");
            _ = facebookLocalServer.StartAsync();

            linkedinLocalServer = new LinkedInLocalOAuthServer("http://localhost:8891/");
            _ = linkedinLocalServer.StartAsync();

            sessionRepository = new SessionRepository(context);

            basicAuthenticationProvider = new BasicAuthenticationProvider(new UserRepository(context));

            userRepository = new UserRepository(context);
        }

        //public AuthenticationService(ILinkedInLocalOAuthServer linkedinLocalServer, IGitHubLocalOAuthServer githubLocalServer, IFacebookLocalOAuthServer facebookLocalServer, IUserRepository userRepository, ISessionRepository sessionAdapter, IBasicAuthenticationProvider basicAuthenticationProvider)
        //{
        //    this.linkedinLocalServer = linkedinLocalServer;
        //    this.githubLocalServer = githubLocalServer;
        //    this.facebookLocalServer = facebookLocalServer;
        //    this.userRepository = userRepository;
        //    this.sessionRepository = sessionAdapter;
        //    this.basicAuthenticationProvider = basicAuthenticationProvider;
        //    _ = githubLocalServer.StartAsync();
        //    _ = facebookLocalServer.StartAsync();
        //    _ = linkedinLocalServer.StartAsync();
        //}

        public async Task<AuthenticationResponse> AuthWithOAuth(OAuthService selectedService, object authProvider)
        {
            var authResponse = selectedService switch
            {
                //OAuthService.Google => await AuthenticateWithGoogleAsync(window, authProvider as IGoogleOAuth2Provider),
                OAuthService.Facebook => await AuthenticateWithFacebookAsync(authProvider as IFacebookOAuthHelper),
                //OAuthService.Twitter => await AuthenticateWithTwitterAsync(window, authProvider as ITwitterOAuth2Provider),
                OAuthService.GitHub => await AuthenticateWithGitHubAsync(authProvider as IGitHubOAuthHelper),
                OAuthService.LinkedIn => await AuthenticateWithLinkedInAsync(authProvider as ILinkedInOAuthHelper),
                _ => throw new ArgumentException("Invalid OAuth service selected"),
            };

            if (authResponse.AuthenticationSuccessful)
            {
                currentSessionId = authResponse.SessionId;
                Session session = sessionRepository.GetSession(currentSessionId).Result;
                currentUserId = session.UserId;
            }

            return authResponse;
        }

        public virtual void Logout()
        {
            sessionRepository.EndSession(currentSessionId);
            currentSessionId = Guid.Empty;
            currentUserId = Guid.Empty;
        }

        public virtual async Task<User> GetUser(Guid sessionId)
        {
            Session session = sessionRepository.GetSession(sessionId).Result;
            User user = userRepository.GetUserById(session.UserId).Result;
            return user ?? throw new UserNotFoundException("User not found");
        }

        public async Task<AuthenticationResponse> AuthWithUserPass(string username, string password)
        {
            try
            {
                if (basicAuthenticationProvider.Authenticate(username, password))
                {
                    User user = await userRepository.GetUserByUsername(username) ?? throw new UserNotFoundException("User not found");
                    Session session = sessionRepository.CreateSession(user.UserId).Result;
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
                User user = new()
                {
                    Username = username,
                    PasswordHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)) ?? throw new Exception("Hashing failed")),
                    UserId = Guid.NewGuid(),
                    TwoFASecret = string.Empty,
                    FullName = username,

                };
                user.AssignedRole = RoleType.User;
                user.NumberOfDeletedReviews = 0;
                user.EmailAddress = "ionutcora66@gmail.com";
                await userRepository.CreateUser(user);
                Session session = sessionRepository.CreateSession(user.UserId).Result;
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

        //private static async Task<AuthenticationResponse> AuthenticateWithGoogleAsync(Window window, IGoogleOAuth2Provider googleProvider)
        //{
        //    return await googleProvider.SignInWithGoogleAsync(window);
        //}

        private static async Task<AuthenticationResponse> AuthenticateWithFacebookAsync(IFacebookOAuthHelper faceBookHelper)
        {
            return await faceBookHelper.AuthenticateAsync();
        }

        //private static async Task<AuthenticationResponse> AuthenticateWithTwitterAsync(Window window, ITwitterOAuth2Provider twitterProvider)
        //{
        //    return await twitterProvider.SignInWithTwitterAsync(window);
        //}

        private static async Task<AuthenticationResponse> AuthenticateWithLinkedInAsync(ILinkedInOAuthHelper linkedInHelper)
        {
            return await linkedInHelper.AuthenticateAsync();
        }
    }
}
