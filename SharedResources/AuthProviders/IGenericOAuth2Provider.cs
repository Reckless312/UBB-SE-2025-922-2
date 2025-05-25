namespace DataAccess.OAuthProviders
{
    public interface IGenericOAuth2Provider
    {
        Task<AuthenticationResponse> Authenticate(string userId, string token);
    }
}
