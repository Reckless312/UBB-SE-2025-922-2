using System;
using System.Reflection;
using System.Threading.Tasks;
using DrinkDb_Auth.AuthProviders.LinkedIn;
using IRepository;
using Moq;
using NUnit.Framework;

namespace Tests.Authentication
{
    [TestFixture]
    public class LinkedInOAuthHelperTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private const string ClientId = "test_client_id";
        private const string ClientSecret = "test_client_secret";
        private const string RedirectUri = "http://localhost:8891/auth";
        private const string Scope = "openid profile email";

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
        }

        //[Test]
        //public void BuildAuthorizeUrl_ReturnsCorrectUrl()
        //{
        //    // Arrange
        //    var helper = new LinkedInOAuthHelper(ClientId, ClientSecret, RedirectUri, Scope);

        //    // Act
        //    var method = typeof(LinkedInOAuthHelper).GetMethod("BuildAuthorizeUrl", BindingFlags.NonPublic | BindingFlags.Instance);
        //    var url = method.Invoke(helper, null) as string;

        //    // Assert
        //    var expectedUrl = $"https://www.linkedin.com/oauth/v2/authorization" +
        //                    $"?response_type=code" +
        //                    $"&client_id={ClientId}" +
        //                    $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
        //                    $"&scope={Uri.EscapeDataString(Scope)}";

        //    Assert.That(url, Is.EqualTo(expectedUrl));
        //}

        //[Test]
        //public void OnCodeReceived_WhenTaskCompletionSourceIsNull_DoesNotThrowException()
        //{
        //    // Arrange
        //    var helper = new LinkedInOAuthHelper(ClientId, ClientSecret, RedirectUri, Scope);
        //    var method = typeof(LinkedInOAuthHelper).GetMethod("OnCodeReceived", BindingFlags.NonPublic | BindingFlags.Instance);

        //    // Act & Assert
        //    Assert.DoesNotThrow(() => method.Invoke(helper, new object[] { "test_code" }));
        //}

        //[Test]
        //public void AuthenticateAsync_StartsProcess()
        //{   
        //    // Arrange
        //    var helper = new LinkedInOAuthHelper(ClientId, ClientSecret, RedirectUri, Scope);
            
        //    // Act & Assert
        //    Assert.DoesNotThrow(() => 
        //    {
        //        // Using reflection to get the task without starting a real browser process
        //        var method = typeof(LinkedInOAuthHelper).GetMethod("BuildAuthorizeUrl", BindingFlags.NonPublic | BindingFlags.Instance);
        //        method.Invoke(helper, null);
        //    });
        //}
    }
} 