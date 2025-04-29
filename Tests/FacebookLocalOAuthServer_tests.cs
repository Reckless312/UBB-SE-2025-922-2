using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DrinkDb_Auth.AuthProviders.Facebook;
using NUnit.Framework;

namespace DrinkDb_Auth.Tests
{
    [TestFixture]
    public class FacebookLocalOAuthServerTests
    {
        private FacebookLocalOAuthServer _server;
        private const string TestServerPrefix = "http://localhost:8898/";//check this if errors when running test
        private const string TestToken = "test_access_token";
        private bool _tokenReceived;
        private string _receivedToken;

        [SetUp]
        public void Setup()
        {
            _server = new FacebookLocalOAuthServer(TestServerPrefix);
            _tokenReceived = false;
            _receivedToken = string.Empty;
            FacebookLocalOAuthServer.OnTokenReceived += TokenReceivedHandler;
        }

        [TearDown]
        public void TearDown()
        {
            FacebookLocalOAuthServer.OnTokenReceived -= TokenReceivedHandler;
            _server.Stop();
        }

        private void TokenReceivedHandler(string token)
        {
            _tokenReceived = true;
            _receivedToken = token;
        }

        [Test]
        public async Task StartAsync_StartsHttpListener()
        {
            var serverTask = Task.Run(() => _server.StartAsync());
            await Task.Delay(100);

            using (var client = new HttpClient())
            {
                try
                {
                    await client.GetAsync(TestServerPrefix);
                    Assert.Pass();
                }
                catch (HttpRequestException)
                {
                    Assert.Fail();
                }
                finally
                {
                    _server.Stop();
                }
            }
        }

        [Test]
        public async Task HandleAuthRequest_ReturnsValidHtmlResponse()
        {
            var serverTask = Task.Run(() => _server.StartAsync());
            await Task.Delay(100);

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{TestServerPrefix}auth");
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.IsTrue(content.Contains("Autentificare cu succes!"));
                _server.Stop();
            }
        }

        [Test]
        public async Task HandleTokenRequest_WithValidToken_InvokesCallback()
        {
            var serverTask = Task.Run(() => _server.StartAsync());
            await Task.Delay(100);

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{TestServerPrefix}token");
                request.Content = new StringContent($"access_token={TestToken}", Encoding.UTF8, "text/plain");
                var response = await client.SendAsync(request);
                await Task.Delay(100);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(_tokenReceived);
                Assert.AreEqual(TestToken, _receivedToken);
                _server.Stop();
            }
        }

        [Test]
        public void GetHtmlResponse_ReturnsValidHtml()
        {
            var method = typeof(FacebookLocalOAuthServer).GetMethod("GetHtmlResponse",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var html = method.Invoke(_server, null) as string;

            Assert.IsNotNull(html);
            Assert.IsTrue(html.Contains("Autentificare cu succes!"));
        }

        [Test]
        public void Stop_StopsHttpListener()
        {
            var serverTask = Task.Run(() => _server.StartAsync());
            Task.Delay(100).Wait();

            _server.Stop();

            using (var client = new HttpClient())
            {
                Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync(TestServerPrefix));
            }
        }

        [Test]
        public async Task HandleTokenRequest_WithHashFragment_ProcessesCorrectly()
        {
            var serverTask = Task.Run(() => _server.StartAsync());
            await Task.Delay(100);

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{TestServerPrefix}token");
                request.Content = new StringContent($"#access_token={TestToken}", Encoding.UTF8, "text/plain");
                var response = await client.SendAsync(request);
                await Task.Delay(100);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(_tokenReceived);
                Assert.AreEqual(TestToken, _receivedToken);
                _server.Stop();
            }
        }

        [Test]
        public async Task HandleInvalidRequest_Returns404()
        {
            var serverTask = Task.Run(() => _server.StartAsync());
            await Task.Delay(100);

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{TestServerPrefix}invalid");
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                _server.Stop();
            }
        }
    }
}