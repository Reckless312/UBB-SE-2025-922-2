using System;
using System.Threading.Tasks;
using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.Service;
using DrinkDb_Auth.Service.Authentication;
using Tests;
using Xunit;

namespace TestTest.Service
{
    public sealed class AuthenticationServiceTests
    {
        private AuthenticationService CreateService(MockBasicAuth basicAuth = null)
        {
            return new AuthenticationService(
                new MockLinkedInServer(),
                new MockGitHubServer(),
                new MockFacebookServer(),
                new MockUserAdapter(),
                new MockSessionAdapter(),
                basicAuth ?? new MockBasicAuth()
            );
        }

        [Fact]
        public async Task AuthWithOAuth_Google()
        {
            var service = CreateService();
            var google = new MockGoogleAuthProvider();
            var id = Guid.NewGuid();
            google.MockId = id;

            var response = await service.AuthWithOAuth(null, OAuthService.Google, google);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = id
            };

            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task AuthWithOAuth_Facebook()
        {
            var service = CreateService();
            var facebook = new MockFacebookAuthProvider();
            var id = Guid.NewGuid();
            facebook.MockId = id;

            var response = await service.AuthWithOAuth(null, OAuthService.Facebook, facebook);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = id
            };

            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task AuthWithOAuth_Twitter()
        {
            var service = CreateService();
            var twitter = new MockTwitterAuthProvider();
            var id = Guid.NewGuid();
            twitter.MockId = id;

            var response = await service.AuthWithOAuth(null, OAuthService.Twitter, twitter);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = id
            };

            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task AuthWithOAuth_GitHub()
        {
            var service = CreateService();
            var github = new MockGitHubAuthProvider();
            var id = Guid.NewGuid();
            github.MockId = id;

            var response = await service.AuthWithOAuth(null, OAuthService.GitHub, github);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = id
            };

            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task AuthWithOAuth_LinkedIn()
        {
            var service = CreateService();
            var linkedin = new MockLinkedInAuthProvider();
            var id = Guid.NewGuid();
            linkedin.MockId = id;

            var response = await service.AuthWithOAuth(null, OAuthService.LinkedIn, linkedin);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = id
            };

            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task AuthWithUserPass_Success()
        {
            var basicAuth = new MockBasicAuth { Succeeds = true };
            var userAdapter = new MockUserAdapter { MockId = Guid.NewGuid(), MockUsername = "testuser" };
            var sessionAdapter = new MockSessionAdapter { MockId = userAdapter.MockId };
            var service = new AuthenticationService(
                new MockLinkedInServer(),
                new MockGitHubServer(),
                new MockFacebookServer(),
                userAdapter,
                sessionAdapter,
                basicAuth
            );

            var response = await service.AuthWithUserPass(userAdapter.MockUsername, string.Empty);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = true,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = userAdapter.MockId
            };

            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task AuthWithUserPass_Fail()
        {
            var basicAuth = new MockBasicAuth { Succeeds = false };
            var service = CreateService(basicAuth);

            var response = await service.AuthWithUserPass(string.Empty, string.Empty);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = false,
                NewAccount = false,
                OAuthToken = string.Empty,
                SessionId = Guid.Empty
            };

            Assert.Equal(expected, response);
        }

        [Fact]
        public async Task AuthWithUserPass_Throws_UserNotFound()
        {
            var basicAuth = new MockBasicAuth { Succeeds = true };
            var userAdapter = new MockUserAdapter { MockId = Guid.Parse("12345678-1234-1234-1234-1234567890ab"), MockUsername = "testuser" };
            var sessionAdapter = new MockSessionAdapter { MockId = userAdapter.MockId };
            var service = new AuthenticationService(
                new MockLinkedInServer(),
                new MockGitHubServer(),
                new MockFacebookServer(),
                userAdapter,
                sessionAdapter,
                basicAuth
            );

            var response = await service.AuthWithUserPass("wronguser", string.Empty);

            var expected = new AuthenticationResponse
            {
                AuthenticationSuccessful = true,
                NewAccount = true,
                OAuthToken = string.Empty,
                SessionId = userAdapter.MockId
            };

            Assert.Equal(expected, response);
        }
    }
}
