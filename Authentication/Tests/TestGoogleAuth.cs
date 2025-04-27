using System;
using System.Collections.Generic;
using System.Linq;
using DrinkDb_Auth.AuthProviders;
using DrinkDb_Auth.AuthProviders.Google;
using DrinkDb_Auth.OAuthProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public sealed class TestGoogleAuth
    {
        [TestMethod]
        public void CheckCreatingAGloballyUniqueIdentifier()
        {
            string identifier = "me";

            Guid result = GoogleOAuth2Provider.CreateGloballyUniqueIdentifier(identifier);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestConstructor()
        {
            GoogleOAuth2Provider provider = new GoogleOAuth2Provider();

            Assert.IsNotNull(provider);
        }

        [TestMethod]
        public void AuthenticateSuccesful()
        {
            GoogleOAuth2Provider provider = new GoogleOAuth2Provider();

            AuthenticationResponse response = provider.Authenticate("me", "token");

            Assert.IsTrue(response.AuthenticationSuccessful);
        }

        [TestMethod]
        public void AuthenticateFailed()
        {
            GoogleOAuth2Provider provider = new GoogleOAuth2Provider();

            AuthenticationResponse response = provider.Authenticate("me", string.Empty);

            Assert.IsFalse(response.AuthenticationSuccessful);
        }
    }
}
