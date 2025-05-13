namespace DrinkDb_Auth.Service.Authentication.Interfaces
{
    using DataAccess.Model.Authentication;

    internal interface ITwoFactorAuthenticationService
    {
        (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) Get2FAValues();
    }
}
