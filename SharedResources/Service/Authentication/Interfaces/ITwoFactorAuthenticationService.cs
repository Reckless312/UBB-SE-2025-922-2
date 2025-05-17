namespace DataAccess.Service.Authentication.Interfaces
{
    using DataAccess.Model.Authentication;

    public interface ITwoFactorAuthenticationService
    {
        (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) Get2FAValues();
    }
}
