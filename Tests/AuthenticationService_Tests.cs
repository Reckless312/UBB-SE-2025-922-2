using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public sealed class AuthenticationService_Tests
    {

        [TestMethod]
        public async Task AuthWithOAuth_Google()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            var google = new MockGoogleAuthProvider();
            var id = Guid.NewGuid();
            google.MockId = id;
            var response = await service.AuthWithOAuth(null, OAuthService.Google, google);

            AuthenticationResponse authResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Assert.AreEqual(response, authResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_Facebook()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            var facebook = new MockFacebookAuthProvider();
            var id = Guid.NewGuid();
            facebook.MockId = id;
            var response = await service.AuthWithOAuth(null, OAuthService.Facebook, facebook);

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_Twitter()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            var twitter = new MockTwitterAuthProvider();
            var id = Guid.NewGuid();
            twitter.MockId = id;
            var response = await service.AuthWithOAuth(null, OAuthService.Twitter, twitter);

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_Github()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            var github = new MockGitHubAuthProvider();
            var id = Guid.NewGuid();
            github.MockId = id;
            var response = await service.AuthWithOAuth(null, OAuthService.GitHub, github);

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_LinkedIn()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            var linkedin = new MockLinkedInAuthProvider();
            var id = Guid.NewGuid();
            linkedin.MockId = id;
            var response = await service.AuthWithOAuth(null, OAuthService.LinkedIn, linkedin);

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public void AuthWithUserPass_Success()
        {
            var basicAuth = new MockBasicAuth();
            basicAuth.Succeeds = true;
            var userAdapter = new MockUserAdapter();
            var mockId = Guid.NewGuid();
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "testuser";
            var sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;

            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), userAdapter, sessionAdapter, basicAuth);

            var response = service.AuthWithUserPass(userAdapter.MockUsername, "");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = true, NewAccount = false, OAuthToken = string.Empty, SessionId = userAdapter.MockId };
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void AuthWithUserPass_Fail()
        {
            var basicAuth = new MockBasicAuth();
            basicAuth.Succeeds = false;

            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), basicAuth);

            var response = service.AuthWithUserPass("", "");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = Guid.Empty };
            Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void AuthWithUserPass_Throws_UserNotFound()
        {
            var basicAuth = new MockBasicAuth();
            basicAuth.Succeeds = true;
            var userAdapter = new MockUserAdapter();
            var mockId = Guid.Parse("12345678-1234-1234-1234-1234567890ab");
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "testuser";
            var sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;

            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), userAdapter, sessionAdapter, basicAuth);

            var response = service.AuthWithUserPass("wronguser", "");

            var expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = true, NewAccount = true, OAuthToken = string.Empty, SessionId = mockId };
        
            Assert.AreEqual(expectedResponse, response);
        }
    }
}
