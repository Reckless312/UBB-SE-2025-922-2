using DataAccess.OAuthProviders;

namespace DrinkDb_Auth.AuthProviders.Google
{
    public interface IGoogleOAuth2Provider
    {
        Task<AuthenticationResponse> Authenticate(string userId, string token);
        Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code);
        string GetAuthorizationUrl();
        Task<AuthenticationResponse> SignInWithGoogleAsync();
    }
}