using System;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.ProxyRepository.AdminDashboard;
using DrinkDb_Auth.ProxyRepository.Authentification;
using IRepository;
using Repository.AdminDashboard;
using Repository.Authentication;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public class GitHubOAuth2Provider : GenericOAuth2Provider
    {
        private IUserRepository userRepository;
        private ISessionRepository sessionRepository;
        private IGitHubHttpHelper gitHubHttpHelper;

        public GitHubOAuth2Provider()
        {
            userRepository = new UserProxyRepository();
            sessionRepository = new SessionProxyRepository();
            gitHubHttpHelper = new GitHubHttpHelper();
        }

        public GitHubOAuth2Provider(IUserRepository userRepository, ISessionRepository sessionAdapter, IGitHubHttpHelper gitHubHttpHelper)
        {
            this.userRepository = userRepository;
            this.sessionRepository = sessionAdapter;
            this.gitHubHttpHelper = gitHubHttpHelper;
        }

        public AuthenticationResponse Authenticate(string? userId, string token)
        {
            try
            {
                var (gitHubId, gitHubLogin) = FetchGitHubUserInfo(token);

                if (string.IsNullOrEmpty(gitHubLogin))
                {
                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = false,
                        OAuthToken = string.Empty,
                        SessionId = Guid.Empty,
                        NewAccount = false
                    };
                }

                // Check if a user exists by using the GitHub username.
                if (UserExists(gitHubLogin))
                {
                    // User exists, so proceed.
                    User user = userRepository.GetUserByUsername(gitHubLogin).Result ?? throw new Exception("User not found");

                    Session session = sessionRepository.CreateSession(user.UserId).Result;

                    return new AuthenticationResponse
                    {
                        AuthenticationSuccessful = true,
                        OAuthToken = token,
                        SessionId = session.SessionId,
                        NewAccount = false
                    };
                }
                else
                {
                    // User does not exist. Insert the new user.
                    Guid newUserId = CreateUserFromGitHub(gitHubLogin);
                    if (newUserId != Guid.Empty)
                    {
                        // Successfully inserted, so login is successful.
                        Session session = sessionRepository.CreateSession(newUserId).Result;
                        return new AuthenticationResponse
                        {
                            AuthenticationSuccessful = true,
                            OAuthToken = token,
                            SessionId = session.SessionId,
                            NewAccount = true
                        };
                    }
                    else
                    {
                        // Insertion failed.
                        return new AuthenticationResponse
                        {
                            AuthenticationSuccessful = false,
                            OAuthToken = token,
                            SessionId = Guid.Empty,
                            NewAccount = false
                        };
                    }
                }
            }
            catch (Exception)
            {
                return new AuthenticationResponse
                {
                    AuthenticationSuccessful = false,
                    OAuthToken = token,
                    SessionId = Guid.Empty,
                    NewAccount = false
                };
            }
        }

        private (string gitHubId, string gitHubLogin) FetchGitHubUserInfo(string token)
        {
            return gitHubHttpHelper.FetchGitHubUserInfo(token);
        }

        private bool UserExists(string gitHubLogin)
        {
            try
            {
                User? user = userRepository.GetUserByUsername(gitHubLogin).Result;
                if (user != null)
                {
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error verifying user: " + exception.Message);
            }
            return false;
        }

        private Guid CreateUserFromGitHub(string gitHubLogin)
        {
            try
            {
                User newUser = new ()
                {
                    UserId = Guid.NewGuid(),
                    Username = gitHubLogin.Trim(),
                    TwoFASecret = string.Empty,
                    PasswordHash = string.Empty,
                };
                userRepository.CreateUser(newUser);
                return newUser.UserId;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error creating user: " + exception.Message);
            }
            return Guid.Empty;
        }
    }
}
