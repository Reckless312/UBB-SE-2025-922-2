using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.AuthProviders.Twitter;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class TestTwitterOAuth2Provider
    {
        private sealed class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpResponseMessage TokenResponse { get; set; } = new();
            public HttpResponseMessage UserInfoResponse { get; set; } = new();
            public Exception? ThrowException { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                if (ThrowException != null)
                {
                    throw ThrowException;
                }

                if (request.RequestUri!.ToString().Contains("token"))
                    return Task.FromResult(TokenResponse);

                if (request.RequestUri.ToString().Contains("users/me"))
                    return Task.FromResult(UserInfoResponse);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        private TwitterOAuth2Provider BuildProvider(MockHttpMessageHandler handler)
        {
            TwitterOAuth2Provider provider = new ();
            HttpClient client = new(handler);
            typeof(TwitterOAuth2Provider)
                .GetField("httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(provider, client);
            return provider;
        }

        [TestMethod]
        public void Authenticate_ShouldReturnSuccess_WhenTokenIsNotEmpty()
        {
            var provider = new TwitterOAuth2Provider();
            var result = provider.Authenticate("id", "token123");
            Assert.IsTrue(result.AuthenticationSuccessful);
            Assert.AreEqual("token123", result.OAuthToken);
        }

        [TestMethod]
        public void Authenticate_ShouldReturnFail_WhenTokenIsEmpty()
        {
            var provider = new TwitterOAuth2Provider();
            var result = provider.Authenticate("id", "");
            Assert.IsFalse(result.AuthenticationSuccessful);
        }

        [TestMethod]
        public void Authenticate_ShouldReturnFail_WhenTokenIsNull()
        {
            var provider = new TwitterOAuth2Provider();
            var result = provider.Authenticate("id", null!);
            Assert.IsFalse(result.AuthenticationSuccessful);
        }

        [TestMethod]
        public void GetAuthorizationUrl_ShouldReturnValidUrl()
        {
            var provider = new TwitterOAuth2Provider();
            string url = provider.GetAuthorizationUrl();
            Assert.IsTrue(url.StartsWith("https://twitter.com/i/oauth2/authorize"));
            Assert.IsTrue(url.Contains("code_challenge"));
            Assert.IsTrue(url.Contains("client_id"));
            Assert.IsTrue(url.Contains("scope"));
            Assert.IsTrue(url.Contains("response_type"));
            Assert.IsTrue(url.Contains("redirect_uri"));
        }

        [TestMethod]
        public void GeneratePkceData_ShouldGenerateValidCodeVerifierAndChallenge()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("GeneratePkceData", BindingFlags.NonPublic | BindingFlags.Instance);

            // Fix: Cast to ValueTuple<string, string> instead of using 'as'
            var result = ((ValueTuple<string, string>)method!.Invoke(provider, null!));

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result.Item1)); // codeVerifier
            Assert.IsFalse(string.IsNullOrEmpty(result.Item2)); // codeChallenge
            Assert.IsTrue(result.Item1.Length >= 43);
            Assert.IsTrue(result.Item1.Length <= 128);
        }

        [TestMethod]
        public void GeneratePkceData_ShouldGenerateDifferentValuesEachTime()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("GeneratePkceData", BindingFlags.NonPublic | BindingFlags.Instance);

            // Fix: Cast to ValueTuple<string, string> instead of using 'as'
            var result1 = ((ValueTuple<string, string>)method!.Invoke(provider, null!));
            var result2 = ((ValueTuple<string, string>)method!.Invoke(provider, null!));

            Assert.AreNotEqual(result1.Item1, result2.Item1); // codeVerifier
            Assert.AreNotEqual(result1.Item2, result2.Item2); // codeChallenge
        }


        [TestMethod]
        public async Task ExchangeCodeForTokenAsync_ShouldFailGracefully_WhenTokenInvalid()
        {
            var mockHandler = new MockHttpMessageHandler
            {
                TokenResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"error\":\"invalid_request\"}")
                }
            };
            var provider = BuildProvider(mockHandler);
            var result = await provider.ExchangeCodeForTokenAsync("badcode");
            Assert.IsFalse(result.AuthenticationSuccessful);
            Assert.AreEqual(Guid.Empty, result.SessionId);
        }

        [TestMethod]
        public async Task ExchangeCodeForTokenAsync_ShouldFail_WhenUserInfoMissing()
        {
            var tokenJson = JsonSerializer.Serialize(new TwitterTokenResponse
            {
                AccessToken = "abc123",
                TokenType = "Bearer",
                ExpiresIn = 7200,
                Scope = "tweet.read users.read"
            });

            var mockHandler = new MockHttpMessageHandler
            {
                TokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(tokenJson)
                },
                UserInfoResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                }
            };
            var provider = BuildProvider(mockHandler);
            var result = await provider.ExchangeCodeForTokenAsync("validcode");
            Assert.IsFalse(result.AuthenticationSuccessful);
        }

        [TestMethod]
        public async Task ExchangeCodeForTokenAsync_ShouldHandleNetworkError()
        {
            var mockHandler = new MockHttpMessageHandler
            {
                ThrowException = new HttpRequestException("Network error")
            };
            var provider = BuildProvider(mockHandler);
            var result = await provider.ExchangeCodeForTokenAsync("anycode");
            Assert.IsFalse(result.AuthenticationSuccessful);
            Assert.AreEqual(Guid.Empty, result.SessionId);
        }

        [TestMethod]
        public async Task ExchangeCodeForTokenAsync_ShouldHandleInvalidJsonResponse()
        {
            var mockHandler = new MockHttpMessageHandler
            {
                TokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("invalid json")
                }
            };
            var provider = BuildProvider(mockHandler);
            var result = await provider.ExchangeCodeForTokenAsync("anycode");
            Assert.IsFalse(result.AuthenticationSuccessful);
            Assert.AreEqual(Guid.Empty, result.SessionId);
        }

        [TestMethod]
        public void ExtractQueryParameter_ShouldExtractParameterFromUrl()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractQueryParameter", BindingFlags.NonPublic | BindingFlags.Instance);

            var url = "http://example.com?code=12345&state=abc";
            var result = method!.Invoke(provider, new object[] { url, "code" }) as string;

            Assert.AreEqual("12345", result);
        }

        [TestMethod]
        public void ExtractQueryParameter_ShouldThrowWhenParameterNotFound()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractQueryParameter", BindingFlags.NonPublic | BindingFlags.Instance);

            var url = "http://example.com?state=abc";
            Assert.ThrowsException<TargetInvocationException>(() =>
                method!.Invoke(provider, new object[] { url, "code" }));
        }

        [TestMethod]
        public void ExtractUserInfoFromIdToken_ShouldParseValidToken()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractUserInfoFromIdToken", BindingFlags.NonPublic | BindingFlags.Instance);

            // Create a valid JWT token
            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"data\":{\"id\":\"123\",\"name\":\"Test User\",\"username\":\"testuser\"}}"));
            var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("signature"));
            var token = $"{header}.{payload}.{signature}";

            var result = method!.Invoke(provider, new object[] { token }) as TwitterUserInfoResponse;

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result.Data.Id);
            Assert.AreEqual("Test User", result.Data.Name);
            Assert.AreEqual("testuser", result.Data.Username);
        }

        [TestMethod]
        public void ExtractUserInfoFromIdToken_ShouldThrowOnInvalidTokenFormat()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractUserInfoFromIdToken", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.ThrowsException<TargetInvocationException>(() =>
                method!.Invoke(provider, new object[] { "invalid.token" }));
        }
    }
}