namespace DataAccess.Service.Authentication
{
    using System;
    using DataAccess.Model.Authentication;
    using DataAccess.Service.Authentication.Components;
    using DataAccess.Service.Authentication.Interfaces;
    using IRepository;
    using OtpNet;

    public class TwoFactorAuthenticationService : ITwoFactorAuthenticationService
    {
        private IUserRepository? userRepository;
        private IKeyGeneration? keyGeneration = new OtpKeyGeneration();
        private User? currentUser;

        private Guid userId;
        private bool isFirstTimeSetup;

        public TwoFactorAuthenticationService(IUserRepository userRepo)
        {
            this.userRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public TwoFactorAuthenticationService(Guid userId, bool isFirstTimeSetup)
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