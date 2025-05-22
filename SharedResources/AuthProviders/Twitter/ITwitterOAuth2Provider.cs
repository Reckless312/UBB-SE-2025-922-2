using DataAccess.OAuthProviders;

namespace DataAccess.AuthProviders.Twitter
{
    public interface ITwitterOAuth2Provider
    {
        AuthenticationResponse Authenticate(string userId, string token);
        Task<AuthenticationResponse> ExchangeCodeForTokenAsync(string code);
        string ExtractQueryParameter(string fullUrl, string targetParameter);
        TwitterUserInfoResponse ExtractUserInfoFromIdToken(string jwtIdToken);
        (string codeVerifier, string codeChallenge) GeneratePkceData();
        string GetAuthorizationUrl();
    }
}