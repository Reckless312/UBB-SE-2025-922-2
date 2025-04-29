using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.AuthProviders.Facebook;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Tests;

namespace DrinkDb_Auth.Tests
{
    [TestFixture]
    public class FacebookOAuth2ProviderTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private FacebookOAuth2Provider _provider;
        private MockUserAdapter _mockUserAdapter;
        private MockSessionAdapter _mockSessionAdapter;
        private const string TestToken = "test_token";
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testSessionId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockUserAdapter = new MockUserAdapter();
            _mockSessionAdapter = new MockSessionAdapter();


            _provider = new FacebookOAuth2Provider();


            var userAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("UserAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            userAdapterField.SetValue(null, _mockUserAdapter);

            var sessionAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("SessionAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            sessionAdapterField.SetValue(null, _mockSessionAdapter);
        }

        [TearDown]
        public void TearDown()
        {

            var userAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("UserAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            userAdapterField.SetValue(null, new UserAdapter());

            var sessionAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("SessionAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            sessionAdapterField.SetValue(null, new SessionAdapter());
        }

        [Test]
        public void Authenticate_WithValidTokenAndNewUser_ReturnsSuccessfulResponse()
        {

            const string fbId = "test_fb_id";
            const string fbName = "John Doe";

            SetupMockHttpResponse($"{{\"id\":\"{fbId}\",\"name\":\"{fbName}\"}}");

            _mockUserAdapter.MockUsername = fbName;
            _mockUserAdapter.MockId = _testUserId;
            _mockSessionAdapter.MockId = _testSessionId;


            var response = _provider.Authenticate("", TestToken);

            Assert.IsTrue(response.AuthenticationSuccessful);
            Assert.IsTrue(response.NewAccount);
            Assert.AreEqual(TestToken, response.OAuthToken);
            Assert.AreEqual(_testSessionId, response.SessionId);
        }

        [Test]
        public void Authenticate_WithValidTokenAndExistingUser_ReturnsSuccessfulResponse()
        {

            const string fbId = "test_fb_id";
            const string fbName = "John Doe";

            SetupMockHttpResponse($"{{\"id\":\"{fbId}\",\"name\":\"{fbName}\"}}");


            _mockUserAdapter.MockUsername = fbName;
            _mockUserAdapter.MockId = _testUserId;
            _mockSessionAdapter.MockId = _testSessionId;


            var response = _provider.Authenticate("", TestToken);


            Assert.IsTrue(response.AuthenticationSuccessful);
            Assert.IsFalse(response.NewAccount);
            Assert.AreEqual(TestToken, response.OAuthToken);
            Assert.AreEqual(_testSessionId, response.SessionId);
        }

        [Test]
        public void Authenticate_WithInvalidToken_ReturnsUnsuccessfulResponse()
        {

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API call failed"));


            var response = _provider.Authenticate("", "invalid_token");


            Assert.IsFalse(response.AuthenticationSuccessful);
            Assert.AreEqual(Guid.Empty, response.SessionId);
        }

        private void SetupMockHttpResponse(string jsonContent)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("graph.facebook.com")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }
    }
}