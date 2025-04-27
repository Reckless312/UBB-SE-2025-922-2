using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DrinkDb_Auth.AuthProviders.LinkedIn;
using NUnit.Framework;

namespace DrinkDb_Auth.Tests
{
    [TestFixture]
    public class LinkedInLocalOAuthServerTests
    {
        private LinkedInLocalOAuthServer _server;
        private const string TestServerPrefix = "http://localhost:8899/";
        private const string TestCode = "test_auth_code";
        private bool _codeReceived;
        private string _receivedCode;

        [SetUp]
        public void Setup()
        {
            _server = new LinkedInLocalOAuthServer(TestServerPrefix);
            _codeReceived = false;
            _receivedCode = string.Empty;
            LinkedInLocalOAuthServer.OnCodeReceived += CodeReceivedHandler;
        }

        [TearDown]
        public void TearDown()
        {
            LinkedInLocalOAuthServer.OnCodeReceived -= CodeReceivedHandler;
            _server.Stop();
        }

        private void CodeReceivedHandler(string code)
        {
            _codeReceived = true;
            _receivedCode = code;
        }

        [Test]
        public async Task StartAsync_StartsHttpListener()
        {
            // Arrange
            var serverTask = Task.Run(() => _server.StartAsync());
            
            // Give the server time to start
            await Task.Delay(100);
            
            // Act & Assert
            using (var client = new HttpClient())
            {
                // Check that it responds to a request
                try
                {
                    await client.GetAsync(TestServerPrefix);
                    // If we get here without exception, the server is running
                    Assert.Pass("Server is running");
                }
                catch (HttpRequestException)
                {
                    Assert.Fail("Server is not running");
                }
                finally
                {
                    _server.Stop();
                }
            }
        }

        [Test]
        public async Task HandleAuthRequest_WithValidCode_SendsHtmlResponse()
        {
            // Arrange
            var serverTask = Task.Run(() => _server.StartAsync());
            
            // Give the server time to start
            await Task.Delay(100);
            
            // Act
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{TestServerPrefix}auth?code={TestCode}");
                
                // Assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsTrue(content.Contains("Authentication successful"));
                
                // Stop the server
                _server.Stop();
            }
        }

        [Test]
        public async Task HandleExchangeRequest_WithValidCode_InvokesCallback()
        {
            // Arrange
            var serverTask = Task.Run(() => _server.StartAsync());
            
            // Give the server time to start
            await Task.Delay(100);
            
            // Act
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{TestServerPrefix}exchange");
                request.Content = new StringContent(TestCode, Encoding.UTF8, "text/plain");
                var response = await client.SendAsync(request);
                
                // Give time for the event to be processed
                await Task.Delay(100);
                
                // Assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(_codeReceived);
                Assert.AreEqual(TestCode, _receivedCode);
                
                // Stop the server
                _server.Stop();
            }
        }

        [Test]
        public void GetHtmlResponse_ReturnsValidHtml()
        {
            // Arrange
            // Using reflection to access the private method
            var method = typeof(LinkedInLocalOAuthServer).GetMethod("GetHtmlResponse", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            var html = method.Invoke(_server, new object[] { TestCode }) as string;
            
            // Assert
            Assert.IsNotNull(html);
            Assert.IsTrue(html.Contains("LinkedIn OAuth Login Successful"));
            Assert.IsTrue(html.Contains(TestCode)); 
        }

        [Test]
        public void Stop_StopsHttpListener()
        {
            // Arrange
            var serverTask = Task.Run(() => _server.StartAsync());
            
            // Give the server time to start
            Task.Delay(100).Wait();
            
            // Act
            _server.Stop();
            
            // Assert - Check that it's not responding
            using (var client = new HttpClient())
            {
                Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync(TestServerPrefix));
            }
        }
    }
} 