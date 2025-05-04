using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DrinkDb_Auth.AuthProviders.Facebook;
using DrinkDb_Auth.Repository.AdminDashboard;
using DrinkDb_Auth.Repository.Authentication;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Tests;

namespace Tests.Authentication
{
    [TestFixture]
    public class FacebookOAuth2ProviderTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private FacebookOAuth2Provider provider;
        private MockUserAdapter mockUserAdapter;
        private MockSessionAdapter mockSessionAdapter;
        private const string TestToken = "test_token";
        private readonly Guid testUserId = Guid.NewGuid();
        private readonly Guid testSessionId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockUserAdapter = new MockUserAdapter();
            mockSessionAdapter = new MockSessionAdapter();


            provider = new FacebookOAuth2Provider();


            var userAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("UserAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            userAdapterField.SetValue(null, mockUserAdapter);

            var sessionAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("SessionAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            sessionAdapterField.SetValue(null, mockSessionAdapter);
        }

        [TearDown]
        public void TearDown()
        {

            var userAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("UserAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            userAdapterField.SetValue(null, new UserRepository());

            var sessionAdapterField = typeof(FacebookOAuth2Provider)
                .GetField("SessionAdapter", BindingFlags.NonPublic | BindingFlags.Static);
            sessionAdapterField.SetValue(null, new SessionRepository());
        }

        private void SetupMockHttpResponse(string jsonContent)
        {
            HttpResponseMessage response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            mockHttpMessageHandler
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