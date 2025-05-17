using System;
using DrinkDb_Auth.AuthProviders.Google;
using Xunit;

namespace TestTest.Authentication.Google
{
    public sealed class GoogleOAuth2ProviderTests
    {
        [Fact]
        public void CheckCreatingAGloballyUniqueIdentifier()
        {
            string identifier = "me";

            Guid result = GoogleOAuth2Provider.CreateGloballyUniqueIdentifier(identifier);

            // Ensure the GUID is not the default (empty) value
            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public void TestConstructor()
        {
            var provider = new GoogleOAuth2Provider();

            Assert.NotNull(provider);
        }

        [Fact]
        public void AuthenticateSuccessful()
        {
            var provider = new GoogleOAuth2Provider();

            var response = provider.Authenticate("me", "token");

            Assert.True(response.AuthenticationSuccessful);
        }

        [Fact]
        public void AuthenticateFailed()
        {
            var provider = new GoogleOAuth2Provider();

            var response = provider.Authenticate("me", string.Empty);

            Assert.False(response.AuthenticationSuccessful);
        }
    }
}
