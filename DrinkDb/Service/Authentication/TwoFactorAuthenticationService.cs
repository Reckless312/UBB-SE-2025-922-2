namespace DrinkDb_Auth.Service.Authentication
{
    using System;
    using DataAccess.Model.Authentication;
    using DrinkDb_Auth.ProxyRepository.AdminDashboard;
    using DrinkDb_Auth.Service.Authentication.Components;
    using DrinkDb_Auth.Service.Authentication.Interfaces;
    using IRepository;
    using Microsoft.UI.Xaml;
    using OtpNet;

    public class TwoFactorAuthenticationService : ITwoFactorAuthenticationService
    {
        private IUserRepository? userRepository = new UserProxyRepository();
        private IKeyGeneration? keyGeneration = new OtpKeyGeneration();
        private User? currentUser;

        private Guid userId;
        private bool isFirstTimeSetup;

        public TwoFactorAuthenticationService(Window? window, Guid userId, bool isFirstTimeSetup)
        {
            this.userId = userId;
            this.isFirstTimeSetup = isFirstTimeSetup;
        }

        public (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) Get2FAValues()
        {
            this.currentUser = this.userRepository?.GetUserById(this.userId).Result ?? throw new ArgumentException("User not found.");

            int keyLength = 42;
            byte[] twoFactorSecret;
            string uniformResourceIdentifier = "";


            if (this.isFirstTimeSetup)
            {
                twoFactorSecret = this.keyGeneration?.GenerateRandomKey(keyLength) ?? throw new InvalidOperationException("Failed to generate 2FA secret.");
                this.currentUser.TwoFASecret = Convert.ToBase64String(twoFactorSecret);
                this.userRepository.UpdateUser(this.currentUser);
                uniformResourceIdentifier = new OtpUri(OtpType.Totp, twoFactorSecret, this.currentUser.Username, "DrinkDB").ToString();
            }
            else
            {
                twoFactorSecret = Convert.FromBase64String(this.currentUser.TwoFASecret ?? string.Empty);
            }

            return (this.currentUser, uniformResourceIdentifier, twoFactorSecret);
        }
    }
}