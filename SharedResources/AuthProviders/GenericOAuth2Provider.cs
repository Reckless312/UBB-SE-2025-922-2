namespace DataAccess.OAuthProviders
{
    public interface GenericOAuth2Provider
    {
        AuthenticationResponse Authenticate(string userId, string token);
    }
}
