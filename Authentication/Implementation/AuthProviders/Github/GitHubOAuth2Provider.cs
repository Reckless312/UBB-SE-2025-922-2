using System;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.OAuthProviders;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public class GitHubOAuth2Provider : GenericOAuth2Provider
    {
        private IUserAdapter userAdapter;
        private ISessionAdapter sessionAdapter;
        private IGitHubHttpHelper gitHubHttpHelper;

        public GitHubOAuth2Provider()
        {
            userAdapter = new UserAdapter();
            sessionAdapter = new SessionAdapter();
            gitHubHttpHelper = new GitHubHttpHelper();
        }

        public GitHubOAuth2Provider(IUserAdapter userAdapter, ISessionAdapter sessionAdapter, IGitHubHttpHelper gitHubHttpHelper)
        {
            this.userAdapter = userAdapter;
            this.sessionAdapter = sessionAdapter;
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
                    User user = userAdapter.GetUserByUsername(gitHubLogin) ?? throw new Exception("User not found");

                    Session session = sessionAdapter.CreateSession(user.UserId);

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
                        Session session = sessionAdapter.CreateSession(newUserId);
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
                User? user = userAdapter.GetUserByUsername(gitHubLogin);
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
                userAdapter.CreateUser(newUser);
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
