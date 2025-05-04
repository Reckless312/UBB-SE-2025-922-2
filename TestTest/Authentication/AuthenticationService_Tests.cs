using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.Service;
using DrinkDb_Auth.Service.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Tests.Authentication
{
    [TestClass]
    public sealed class AuthenticationService_Tests
    {
        
        [TestMethod]
        public async Task AuthWithOAuth_Google()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            MockGoogleAuthProvider google = new MockGoogleAuthProvider();
            Guid id = Guid.NewGuid();
            google.MockId = id;
            AuthenticationResponse response = await service.AuthWithOAuth(null, OAuthService.Google, google);

            AuthenticationResponse authResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(response, authResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_Facebook()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            MockFacebookAuthProvider facebook = new MockFacebookAuthProvider();
            Guid id = Guid.NewGuid();
            facebook.MockId = id;
            AuthenticationResponse response = await service.AuthWithOAuth(null, OAuthService.Facebook, facebook);

            AuthenticationResponse expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_Twitter()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            MockTwitterAuthProvider twitter = new MockTwitterAuthProvider();
            Guid id = Guid.NewGuid();
            twitter.MockId = id;
            AuthenticationResponse response = await service.AuthWithOAuth(null, OAuthService.Twitter, twitter);

            AuthenticationResponse expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_Github()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            MockGitHubAuthProvider github = new MockGitHubAuthProvider();
            Guid id = Guid.NewGuid();
            github.MockId = id;
            AuthenticationResponse response = await service.AuthWithOAuth(null, OAuthService.GitHub, github);

            AuthenticationResponse expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public async Task AuthWithOAuth_LinkedIn()
        {
            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), new MockBasicAuth());
            MockLinkedInAuthProvider linkedin = new MockLinkedInAuthProvider();
            Guid id = Guid.NewGuid();
            linkedin.MockId = id;
            AuthenticationResponse response = await service.AuthWithOAuth(null, OAuthService.LinkedIn, linkedin);

            AuthenticationResponse expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = id };

            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(response, expectedResponse);
        }

        [TestMethod]
        public void AuthWithUserPass_Success()
        {
            MockBasicAuth basicAuth = new MockBasicAuth();
            basicAuth.Succeeds = true;
            MockUserAdapter userAdapter = new MockUserAdapter();
            Guid mockId = Guid.NewGuid();
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "testuser";
            MockSessionAdapter sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;

            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), userAdapter, sessionAdapter, basicAuth);

            AuthenticationResponse response = service.AuthWithUserPass(userAdapter.MockUsername, "");

            AuthenticationResponse expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = true, NewAccount = false, OAuthToken = string.Empty, SessionId = userAdapter.MockId };
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void AuthWithUserPass_Fail()
        {
            MockBasicAuth basicAuthentication = new MockBasicAuth();
            basicAuthentication.Succeeds = false;

            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), new MockUserAdapter(), new MockSessionAdapter(), basicAuthentication);

            AuthenticationResponse response = service.AuthWithUserPass("", "");

            AuthenticationResponse expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = string.Empty, SessionId = Guid.Empty };
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedResponse, response);
        }

        [TestMethod]
        public void AuthWithUserPass_Throws_UserNotFound()
        {
            MockBasicAuth basicAuthentication = new MockBasicAuth();
            basicAuthentication.Succeeds = true;
            MockUserAdapter userAdapter = new MockUserAdapter();
            Guid mockId = Guid.Parse("12345678-1234-1234-1234-1234567890ab");
            userAdapter.MockId = mockId;
            userAdapter.MockUsername = "testuser";
            MockSessionAdapter sessionAdapter = new MockSessionAdapter();
            sessionAdapter.MockId = mockId;

            AuthenticationService service = new AuthenticationService(new MockLinkedInServer(), new MockGitHubServer(), new MockFacebookServer(), userAdapter, sessionAdapter, basicAuthentication);

            AuthenticationResponse response = service.AuthWithUserPass("wronguser", "");

            AuthenticationResponse expectedResponse = new AuthenticationResponse { AuthenticationSuccessful = true, NewAccount = true, OAuthToken = string.Empty, SessionId = mockId };
        
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedResponse, response);
        }
    }
}
