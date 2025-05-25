namespace DataAccess.OAuthProviders
{
    public class AuthenticationResponse
    {
        public required bool AuthenticationSuccessful { get; set; }

        public required Guid SessionId { get; set; }

        public required string? OAuthToken { get; set; }

        public required bool NewAccount { get; set; }

        public override bool Equals(object other)
        {
            AuthenticationResponse? otherResponse = other as AuthenticationResponse;
            if (otherResponse == null)
            {
                return false;
            }
            return AuthenticationSuccessful == otherResponse.AuthenticationSuccessful && SessionId == otherResponse.SessionId && OAuthToken == otherResponse.OAuthToken && NewAccount == otherResponse.NewAccount;
        }
    }
}
