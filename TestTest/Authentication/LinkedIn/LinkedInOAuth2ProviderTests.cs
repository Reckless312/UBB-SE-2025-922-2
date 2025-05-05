using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DataAccess.Model;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.OAuthProviders;
using IRepository;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace TestTest.Authentication.LinkedIn
{
    [TestFixture]
    public class LinkedInOAuth2ProviderTests
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<ISessionRepository> _mockSessionRepository;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private TestableLinkedInOAuth2Provider _provider;
        private const string TestToken = "test_token";
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testSessionId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockSessionRepository = new Mock<ISessionRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Create HttpClient with mock handler
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            // Use our testable provider that accepts mock dependencies
            _provider = new TestableLinkedInOAuth2Provider(
                httpClient,
                _mockUserRepository.Object,
                _mockSessionRepository.Object);
        }

        //[Test]
        //public void Authenticate_WithValidTokenAndNewUser_ReturnsSuccessfulResponse()
        //{
        //    // Arrange
        //    const string userId = "test_linkedin_id";
        //    const string userName = "John Doe";
            
        //    // Mock HTTP response for LinkedIn API
        //    SetupMockHttpResponse("{\"sub\":\"" + userId + "\",\"name\":\"" + userName + "\"}");
            
        //    // Setup mock for user not existing
        //    _mockUserRepository.Setup(m => m.GetUserByUsername(userName)).Returns((User)null);
            
        //    // Setup mock for creating new user
        //    _mockUserRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns(true);
            
        //    // Setup mock for creating session
        //    _mockSessionRepository.Setup(m => m.CreateSession(It.IsAny<Guid>())).Returns(new Session
        //    {
        //        SessionId = _testSessionId,
        //        UserId = _testUserId
        //    });
            
        //    // Act
        //    var response = _provider.Authenticate("", TestToken);
            
        //    // Assert
        //    NUnit.Framework.Assert.Equals(response.AuthenticationSuccessful, true);
        //    NUnit.Framework.Assert.Equals(response.NewAccount, true);
        //    NUnit.Framework.Assert.Equals(TestToken, response.OAuthToken);
        //}

        //[Test]
        //public void Authenticate_WithValidTokenAndExistingUser_ReturnsSuccessfulResponse()
        //{
        //    // Arrange
        //    const string userId = "test_linkedin_id";
        //    const string userName = "John Doe";
            
        //    // Mock HTTP response for LinkedIn API
        //    SetupMockHttpResponse("{\"sub\":\"" + userId + "\",\"name\":\"" + userName + "\"}");
            
        //    // Setup mock for existing user
        //    var existingUser = new User
        //    {
        //        UserId = _testUserId,
        //        Username = userName,
        //        PasswordHash = "hash",
        //        TwoFASecret = null
        //    };
            
        //    _mockUserRepository.Setup(m => m.GetUserByUsername(userName)).Returns(existingUser);
            
        //    // Setup mock for creating session
        //    _mockSessionRepository.Setup(m => m.CreateSession(_testUserId)).Returns(new Session
        //    {
        //        SessionId = _testSessionId,
        //        UserId = _testUserId
        //    });
            
        //    // Act
        //    var response = _provider.Authenticate("", TestToken);
            
        //    // Assert
        //    Assert.IsTrue(response.AuthenticationSuccessful);
        //    Assert.IsFalse(response.NewAccount);
        //    Assert.AreEqual(TestToken, response.OAuthToken);
        //}

        //[Test]
        //public void Authenticate_WithInvalidToken_ReturnsUnsuccessfulResponse()
        //{
        //    // Arrange
        //    // Setup mock to throw exception when calling LinkedIn API
        //    _mockHttpMessageHandler
        //        .Protected()
        //        .Setup<Task<HttpResponseMessage>>(
        //            "SendAsync",
        //            ItExpr.IsAny<HttpRequestMessage>(),
        //            ItExpr.IsAny<CancellationToken>())
        //        .ThrowsAsync(new HttpRequestException("API call failed"));
            
        //    // We still need basic setup for user and session adapters
        //    _mockUserRepository.Setup(m => m.GetUserByUsername(It.IsAny<string>())).Returns((User)null);
        //    _mockUserRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns(true);
        //    _mockSessionRepository.Setup(m => m.CreateSession(It.IsAny<Guid>())).Returns(new Session
        //    {
        //        SessionId = _testSessionId,
        //        UserId = _testUserId
        //    });
            
        //    // Act & Assert
        //    try 
        //    {
        //        _provider.Authenticate("", "invalid_token");
        //        Assert.Fail("Expected HttpRequestException was not thrown");
        //    }
        //    catch (HttpRequestException)
        //    {
        //        // This is the expected exception
        //        Assert.Pass();
        //    }
        //    catch (AggregateException ex) when (ex.InnerException is HttpRequestException)
        //    {
        //        // Also acceptable if the HttpRequestException is wrapped
        //        Assert.Pass();
        //    }
        //}

        //[Test]
        //public void Authenticate_WithEmptyOrNullId_ReturnsUnsuccessfulResponse()
        //{
        //    // Arrange
        //    // Mock HTTP response with missing/empty fields
        //    SetupMockHttpResponse("{\"sub\":\"\",\"name\":\"John Doe\"}");
            
        //    // We still need basic setup for user and session adapters
        //    _mockUserRepository.Setup(m => m.GetUserByUsername(It.IsAny<string>())).Returns((User)null);
        //    _mockUserRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns(true);
        //    _mockSessionRepository.Setup(m => m.CreateSession(It.IsAny<Guid>())).Returns(new Session
        //    {
        //        SessionId = _testSessionId,
        //        UserId = _testUserId
        //    });
            
        //    // Act
        //    var response = _provider.Authenticate("", TestToken);
            
        //    // Assert
        //    Assert.IsFalse(response.AuthenticationSuccessful);
        //    Assert.AreEqual(string.Empty, response.OAuthToken);
        //    Assert.AreEqual(Guid.Empty, response.SessionId);
        //}

        //private void SetupMockHttpResponse(string jsonContent)
        //{
        //    var response = new HttpResponseMessage
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Content = new StringContent(jsonContent)
        //    };

        //    _mockHttpMessageHandler
        //        .Protected()
        //        .Setup<Task<HttpResponseMessage>>(
        //            "SendAsync",
        //            ItExpr.IsAny<HttpRequestMessage>(),
        //            ItExpr.IsAny<CancellationToken>())
        //        .ReturnsAsync(response);
        //}
    }
} 