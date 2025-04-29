using System;

namespace DrinkDb_Auth.OAuthProviders
{
    /// Represents the result of an OAuth2 authentication attempt.
    public class AuthenticationResponse
    {
        /// True if authentication succeeded, false otherwise.
        public required bool AuthenticationSuccessful { get; set; }

        /// The session id
        public required Guid SessionId { get; set; }

        /// The session token from the OAuth provider.
        public required string? OAuthToken { get; set; }

        /// Indicates whether this authentication created a brand new account.
        public required bool NewAccount { get; set; }

        public override bool Equals(object other)
        {
            var otherResponse = other as AuthenticationResponse;
            if (otherResponse == null)
            {
                return false;
            }
            return AuthenticationSuccessful == otherResponse.AuthenticationSuccessful && SessionId == otherResponse.SessionId && OAuthToken == otherResponse.OAuthToken && NewAccount == otherResponse.NewAccount;
        }
    }
}
