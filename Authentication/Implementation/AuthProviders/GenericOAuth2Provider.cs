namespace DrinkDb_Auth.OAuthProviders
{
    /// <summary>
    /// A generic OAuth2 contract that various providers (Facebook, Google, etc.) can implement.
    /// </summary>
    public interface GenericOAuth2Provider
    {
        /// <summary>
        /// Authenticates a user using the provided token.
        /// Returns an AuthResponse indicating success or failure.
        /// </summary>
        /// <param name="userId">The user identifier (if applicable to your flow).</param>
        /// <param name="token">The OAuth2 token (e.g. an access token) to validate or use.</param>
        /// <returns>An AuthResponse with the authentication result.</returns>
        AuthenticationResponse Authenticate(string userId, string token);
    }
}
