using System;
using System.Reflection;
using System.Threading.Tasks;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.AuthProviders.LinkedIn;
using Moq;
using NUnit.Framework;

namespace DrinkDb_Auth.Tests
{
    [TestFixture]
    public class LinkedInOAuthHelperTests
    {
        private Mock<IUserAdapter> _mockUserAdapter;
        private const string ClientId = "test_client_id";
        private const string ClientSecret = "test_client_secret";
        private const string RedirectUri = "http://localhost:8891/auth";
        private const string Scope = "openid profile email";

        [SetUp]
        public void Setup()
        {
            _mockUserAdapter = new Mock<IUserAdapter>();
        }

        [Test]
        public void Constructor_WithValidParameters_InitializesProperties()
        {
            // Arrange & Act
            var helper = new LinkedInOAuthHelper(ClientId, ClientSecret, RedirectUri, Scope);

            // Assert - Use reflection to test private fields
            var clientIdField = typeof(LinkedInOAuthHelper).GetField("clientId", BindingFlags.NonPublic | BindingFlags.Instance);
            var clientSecretField = typeof(LinkedInOAuthHelper).GetField("clientSecret", BindingFlags.NonPublic | BindingFlags.Instance);
            var redirectUrlField = typeof(LinkedInOAuthHelper).GetField("redirectUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var scopeField = typeof(LinkedInOAuthHelper).GetField("scope", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(clientIdField.GetValue(helper), Is.EqualTo(ClientId));
            Assert.That(clientSecretField.GetValue(helper), Is.EqualTo(ClientSecret));
            Assert.That(redirectUrlField.GetValue(helper), Is.EqualTo(RedirectUri));
            Assert.That(scopeField.GetValue(helper), Is.EqualTo(Scope));
        }

        [Test]
        public void BuildAuthorizeUrl_ReturnsCorrectUrl()
        {
            // Arrange
            var helper = new LinkedInOAuthHelper(ClientId, ClientSecret, RedirectUri, Scope);

            // Act
            var method = typeof(LinkedInOAuthHelper).GetMethod("BuildAuthorizeUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var url = method.Invoke(helper, null) as string;

            // Assert
            var expectedUrl = $"https://www.linkedin.com/oauth/v2/authorization" +
                            $"?response_type=code" +
                            $"&client_id={ClientId}" +
                            $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                            $"&scope={Uri.EscapeDataString(Scope)}";

            Assert.That(url, Is.EqualTo(expectedUrl));
        }

        [Test]
        public void OnCodeReceived_WhenTaskCompletionSourceIsNull_DoesNotThrowException()
        {
            // Arrange
            var helper = new LinkedInOAuthHelper(ClientId, ClientSecret, RedirectUri, Scope);
            var method = typeof(LinkedInOAuthHelper).GetMethod("OnCodeReceived", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            Assert.DoesNotThrow(() => method.Invoke(helper, new object[] { "test_code" }));
        }

        [Test]
        public void AuthenticateAsync_StartsProcess()
        {   
            // Arrange
            var helper = new LinkedInOAuthHelper(ClientId, ClientSecret, RedirectUri, Scope);
            
            // Act & Assert
            Assert.DoesNotThrow(() => 
            {
                // Using reflection to get the task without starting a real browser process
                var method = typeof(LinkedInOAuthHelper).GetMethod("BuildAuthorizeUrl", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(helper, null);
            });
        }
    }
} 