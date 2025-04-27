using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.AuthProviders.Github;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Tests
{
    [TestClass]
    public class GitHubOAuth2Provider_Tests
    {
        [TestMethod]
        public void Authenticate_EmptyLogin()
        {
            var httpHelper = new MockGitHubHttpHelper();
            httpHelper.MockGitHubLogin = string.Empty;
            httpHelper.MockGitHubId = string.Empty;
            var userAdapter = new MockUserAdapter();
            var sessionAdapter = new MockSessionAdapter();
            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = Guid.Empty };
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void Authenticate_ExistingUser()
        {
            var mockId = Guid.NewGuid();
            var httpHelper = new MockGitHubHttpHelper();
            httpHelper.MockGitHubLogin = "testuser";
            httpHelper.MockGitHubId = string.Empty;
            var userAdapter = new MockUserAdapter();
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "testuser";
            userAdapter.MockId = mockId;
            var sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;

            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = true, NewAccount = false, OAuthToken = string.Empty, SessionId = mockId };
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void Authenticate_NonExistingUser()
        {
            var mockId = Guid.NewGuid();
            var httpHelper = new MockGitHubHttpHelper();
            httpHelper.MockGitHubLogin = "testuser";
            httpHelper.MockGitHubId = string.Empty;
            var userAdapter = new MockUserAdapter();
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "wronguser";
            userAdapter.MockId = mockId;
            var sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;

            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "testtoken");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = true, NewAccount = true, OAuthToken = "testtoken", SessionId = mockId };
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void Authenticate_NonExistingUser_InsertFail()
        {
            var mockId = Guid.NewGuid();
            var httpHelper = new MockGitHubHttpHelper();
            httpHelper.MockGitHubLogin = "testuser";
            httpHelper.MockGitHubId = string.Empty;
            var userAdapter = new MockUserAdapter();
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "wronguser";
            userAdapter.MockId = mockId;
            userAdapter.Throws = true;
            var sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;

            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "testtoken");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = "testtoken", SessionId = Guid.Empty };
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void Authenticate_Throws()
        {
            var mockId = Guid.NewGuid();
            var httpHelper = new MockGitHubHttpHelper();
            httpHelper.MockGitHubLogin = "testuser";
            httpHelper.MockGitHubId = string.Empty;
            var userAdapter = new MockUserAdapter();
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "wronguser";
            userAdapter.MockId = mockId;
            userAdapter.Throws = true;
            var sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;
            httpHelper.Throws = true;

            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "testtoken");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = "testtoken", SessionId = Guid.Empty };
            Assert.AreEqual(expectedResponse, response);
        }
    }
}
