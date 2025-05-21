using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataAccess.AuthProviders;
using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.Github;
using DataAccess.AuthProviders.LinkedIn;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using IRepository;
using DataAccess.Service.Authentication.Interfaces;
using DataAccess.Model.AdminDashboard;
using DataAccess.AuthProviders.Twitter;

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
        private readonly ISessionRepository sessionRepository;
        private readonly IUserRepository userRepository;
        private readonly ILinkedInLocalOAuthServer linkedinLocalServer;
        private readonly IGitHubLocalOAuthServer githubLocalServer;
        private readonly IFacebookLocalOAuthServer facebookLocalServer;
        private readonly IBasicAuthenticationProvider basicAuthenticationProvider;
        private static Guid currentSessionId = Guid.Empty;
        private static Guid currentUserId = Guid.Empty;

        public AuthenticationService(
            ISessionRepository sessionRepository,
            IUserRepository userRepository,
            ILinkedInLocalOAuthServer linkedinLocalServer,
            IGitHubLocalOAuthServer githubLocalServer,
            IFacebookLocalOAuthServer facebookLocalServer,
            IBasicAuthenticationProvider basicAuthenticationProvider)
        {
            System.Diagnostics.Debug.WriteLine("AuthenticationService: Constructor called with original implementation");
            this.sessionRepository = sessionRepository;
            this.userRepository = userRepository;
            this.linkedinLocalServer = linkedinLocalServer;
            this.githubLocalServer = githubLocalServer;
            this.facebookLocalServer = facebookLocalServer;
            this.basicAuthenticationProvider = basicAuthenticationProvider;

            _ = githubLocalServer.StartAsync();
            _ = facebookLocalServer.StartAsync();
            _ = linkedinLocalServer.StartAsync();
        }

        public async Task<AuthenticationResponse> AuthWithOAuth(OAuthService selectedService, object authProvider)
        {
            AuthenticationResponse authResponse = selectedService switch
            {
                //OAuthService.Google => await AuthenticateWithGoogleAsync(window, authProvider as IGoogleOAuth2Provider),
                OAuthService.Facebook => await AuthenticateWithFacebookAsync(authProvider as IFacebookOAuthHelper),
                //OAuthService.Twitter => await AuthenticateWithTwitterAsync(authProvider as ITwitterOAuth2Provider),
                OAuthService.GitHub => await AuthenticateWithGitHubAsync(authProvider as IGitHubOAuthHelper),
                OAuthService.LinkedIn => await AuthenticateWithLinkedInAsync(authProvider as ILinkedInOAuthHelper),
                _ => throw new ArgumentException("Invalid OAuth service selected"),
            };

            if (authResponse.AuthenticationSuccessful)
            {
                currentSessionId = authResponse.SessionId;
                Session session = await sessionRepository.GetSession(currentSessionId);
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
            Session session = await sessionRepository.GetSession(sessionId);
            User user = await userRepository.GetUserById(session.UserId);
            return user ?? throw new UserNotFoundException("User not found");
        }

        public async Task<AuthenticationResponse> AuthWithUserPass(string username, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AuthenticationService: Attempting to authenticate user {username}");
                if (await basicAuthenticationProvider.AuthenticateAsync(username, password))
                {
                    System.Diagnostics.Debug.WriteLine("AuthenticationService: Basic authentication successful");
                    User user = await userRepository.GetUserByUsername(username) ?? throw new UserNotFoundException("User not found");
                    System.Diagnostics.Debug.WriteLine($"AuthenticationService: Found user {user.UserId}");
                    Session session = await sessionRepository.CreateSession(user.UserId);
                    System.Diagnostics.Debug.WriteLine($"AuthenticationService: Created session {session.SessionId}");
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
                    System.Diagnostics.Debug.WriteLine("AuthenticationService: Basic authentication failed");
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
                System.Diagnostics.Debug.WriteLine("AuthenticationService: User not found, creating new account");
                // create user
                User user = new()
                {
                    Username = username,
                    PasswordHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)) ?? throw new Exception("Hashing failed")),
                    UserId = Guid.NewGuid(),
                    TwoFASecret = string.Empty,
                    FullName = username,
                    AssignedRole = RoleType.User,
                    NumberOfDeletedReviews = 0,
                    EmailAddress = "ionutcora66@gmail.com"
                };

                await userRepository.CreateUser(user);
                System.Diagnostics.Debug.WriteLine($"AuthenticationService: Created new user {user.UserId}");
                Session session = await sessionRepository.CreateSession(user.UserId);
                System.Diagnostics.Debug.WriteLine($"AuthenticationService: Created session {session.SessionId} for new user");
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = true,
                    NewAccount = true,
                    OAuthToken = string.Empty,
                    SessionId = session.SessionId,
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuthenticationService: Unexpected error: {ex}");
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = false,
                    NewAccount = false,
                    OAuthToken = string.Empty,
                    SessionId = Guid.Empty,
                };
            }
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

        //private static async Task<AuthenticationResponse> AuthenticateWithTwitterAsync(ITwitterOAuth2Provider twitterProvider)
        //{
        //    return await twitterProvider.SignInWithTwitterAsync();
        //}

        private static async Task<AuthenticationResponse> AuthenticateWithLinkedInAsync(ILinkedInOAuthHelper linkedInHelper)
        {
            return await linkedInHelper.AuthenticateAsync();
        }
    }
}
