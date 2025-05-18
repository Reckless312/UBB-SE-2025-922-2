namespace DataAccess.Service.Authentication.Interfaces
{
    using DataAccess.Model.Authentication;

    public interface ITwoFactorAuthenticationService
    {
        Guid UserId { get; set; }
        bool IsFirstTimeSetup { get; set; }

        (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) Get2FAValues();
    }
}
