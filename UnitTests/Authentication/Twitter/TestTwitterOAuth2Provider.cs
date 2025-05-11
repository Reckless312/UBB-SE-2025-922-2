using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.AuthProviders.Twitter;
using DataAccess.Model;
using Xunit;

namespace TestTest.Authentication.Twitter
{
    public class TwitterOAuth2ProviderTests
    {
        private sealed class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpResponseMessage TokenResponse { get; set; } = new();
            public HttpResponseMessage UserInfoResponse { get; set; } = new();
            public Exception? ThrowException { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (ThrowException != null)
                {
                    throw ThrowException;
                }

                var uri = request.RequestUri?.ToString() ?? string.Empty;
                if (uri.Contains("token"))
                    return Task.FromResult(TokenResponse);

                if (uri.Contains("users/me"))
                    return Task.FromResult(UserInfoResponse);

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        private TwitterOAuth2Provider BuildProvider(MockHttpMessageHandler handler)
        {
            var provider = new TwitterOAuth2Provider();
            var client = new HttpClient(handler);
            typeof(TwitterOAuth2Provider)
                .GetField("httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(provider, client);
            return provider;
        }

        [Fact]
        public void Authenticate_ShouldReturnSuccess_WhenTokenIsNotEmpty()
        {
            var provider = new TwitterOAuth2Provider();
            var result = provider.Authenticate("id", "token123");

            Assert.True(result.AuthenticationSuccessful);
            Assert.Equal("token123", result.OAuthToken);
        }

        [Fact]
        public void Authenticate_ShouldReturnFail_WhenTokenIsEmpty()
        {
            var provider = new TwitterOAuth2Provider();
            var result = provider.Authenticate("id", string.Empty);

            Assert.False(result.AuthenticationSuccessful);
        }

        [Fact]
        public void Authenticate_ShouldReturnFail_WhenTokenIsNull()
        {
            var provider = new TwitterOAuth2Provider();
            var result = provider.Authenticate("id", null!);

            Assert.False(result.AuthenticationSuccessful);
        }

        [Fact]
        public void GetAuthorizationUrl_ShouldReturnValidUrl()
        {
            var provider = new TwitterOAuth2Provider();
            string url = provider.GetAuthorizationUrl();

            Assert.StartsWith("https://twitter.com/i/oauth2/authorize", url);
            Assert.Contains("code_challenge", url);
            Assert.Contains("client_id", url);
            Assert.Contains("scope", url);
            Assert.Contains("response_type", url);
            Assert.Contains("redirect_uri", url);
        }

        [Fact]
        public void GeneratePkceData_ShouldGenerateValidCodeVerifierAndChallenge()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("GeneratePkceData", BindingFlags.NonPublic | BindingFlags.Instance);

            var (codeVerifier, codeChallenge) = ((string, string))method!.Invoke(provider, null!);

            Assert.False(string.IsNullOrEmpty(codeVerifier));
            Assert.False(string.IsNullOrEmpty(codeChallenge));
            Assert.InRange(codeVerifier.Length, 43, 128);
        }

        [Fact]
        public void GeneratePkceData_ShouldGenerateDifferentValuesEachTime()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("GeneratePkceData", BindingFlags.NonPublic | BindingFlags.Instance);

            var result1 = ((string, string))method!.Invoke(provider, null!);
            var result2 = ((string, string))method!.Invoke(provider, null!);

            Assert.NotEqual(result1.Item1, result2.Item1);
            Assert.NotEqual(result1.Item2, result2.Item2);
        }

        [Fact]
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

            Assert.False(result.AuthenticationSuccessful);
            Assert.Equal(Guid.Empty, result.SessionId);
        }

        [Fact]
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

            Assert.False(result.AuthenticationSuccessful);
        }

        [Fact]
        public async Task ExchangeCodeForTokenAsync_ShouldHandleNetworkError()
        {
            var mockHandler = new MockHttpMessageHandler
            {
                ThrowException = new HttpRequestException("Network error")
            };
            var provider = BuildProvider(mockHandler);

            var result = await provider.ExchangeCodeForTokenAsync("anycode");

            Assert.False(result.AuthenticationSuccessful);
            Assert.Equal(Guid.Empty, result.SessionId);
        }

        [Fact]
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

            Assert.False(result.AuthenticationSuccessful);
            Assert.Equal(Guid.Empty, result.SessionId);
        }

        [Fact]
        public void ExtractQueryParameter_ShouldExtractParameterFromUrl()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractQueryParameter", BindingFlags.NonPublic | BindingFlags.Instance);

            var url = "http://example.com?code=12345&state=abc";
            var result = method!.Invoke(provider, new object[] { url, "code" }) as string;

            Assert.Equal("12345", result);
        }

        [Fact]
        public void ExtractQueryParameter_ShouldThrowWhenParameterNotFound()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractQueryParameter", BindingFlags.NonPublic | BindingFlags.Instance);

            var url = "http://example.com?state=abc";
            Assert.Throws<TargetInvocationException>(() =>
                method!.Invoke(provider, new object[] { url, "code" }));
        }

        [Fact]
        public void ExtractUserInfoFromIdToken_ShouldParseValidToken()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractUserInfoFromIdToken", BindingFlags.NonPublic | BindingFlags.Instance);

            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"data\":{\"id\":\"123\",\"name\":\"Test User\",\"username\":\"testuser\"}}"));
            var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("signature"));
            var token = $"{header}.{payload}.{signature}";

            var result = method!.Invoke(provider, new object[] { token }) as TwitterUserInfoResponse;

            Assert.NotNull(result);
            Assert.Equal("123", result!.Data.Id);
            Assert.Equal("Test User", result.Data.Name);
            Assert.Equal("testuser", result.Data.Username);
        }

        [Fact]
        public void ExtractUserInfoFromIdToken_ShouldThrowOnInvalidTokenFormat()
        {
            var provider = new TwitterOAuth2Provider();
            var method = typeof(TwitterOAuth2Provider)
                .GetMethod("ExtractUserInfoFromIdToken", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.Throws<TargetInvocationException>(() =>
                method!.Invoke(provider, new object[] { "invalid.token" }));
        }
    }
}
