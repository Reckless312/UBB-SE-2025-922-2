using System;
using Tests;
using Xunit;
using DataAccess.AuthProviders.Github;
using DataAccess.OAuthProviders;

namespace TestTest.Authentication.Github
{
    public class GitHubOAuth2ProviderTests
    {
        [Fact]
        public void Authenticate_EmptyLogin()
        {
            var httpHelper = new MockGitHubHttpHelper
            {
                MockGitHubLogin = string.Empty,
                MockGitHubId = string.Empty
            };
            var userAdapter = new MockUserAdapter();
            var sessionAdapter = new MockSessionAdapter();
            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "");

            var expectedResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = Guid.Empty
            };

            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public void Authenticate_NonExistingUser_InsertFail()
        {
            var mockId = Guid.NewGuid();
            var httpHelper = new MockGitHubHttpHelper
            {
                MockGitHubLogin = "testuser",
                MockGitHubId = string.Empty
            };
            var userAdapter = new MockUserAdapter
            {
                MockId = mockId,
                MockUsername = "wronguser",
                Throws = true
            };
            var sessionAdapter = new MockSessionAdapter { MockId = mockId };
            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "testtoken");

            var expectedResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = "testtoken",
                SessionId = Guid.Empty
            };

            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public void Authenticate_Throws()
        {
            var mockId = Guid.NewGuid();
            var httpHelper = new MockGitHubHttpHelper
            {
                MockGitHubLogin = "testuser",
                MockGitHubId = string.Empty,
                Throws = true
            };
            var userAdapter = new MockUserAdapter
            {
                MockId = mockId,
                MockUsername = "wronguser",
                Throws = true
            };
            var sessionAdapter = new MockSessionAdapter { MockId = mockId };
            var gitHubOAuth2Provider = new GitHubOAuth2Provider(userAdapter, sessionAdapter, httpHelper);

            var response = gitHubOAuth2Provider.Authenticate("", "testtoken");

            var expectedResponse = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = "testtoken",
                SessionId = Guid.Empty
            };

            Assert.Equal(expectedResponse, response);
        }
    }
}