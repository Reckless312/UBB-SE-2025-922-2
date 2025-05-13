using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;
using IRepository;
using DrinkDb_Auth.Service.Authentication.Components;
using DrinkDb_Auth.Service.Authentication.Interfaces;
using DrinkDb_Auth.View;
using DrinkDb_Auth.View.Authentication;
using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel;
using DrinkDb_Auth.ViewModel.AdminDashboard.Components;
using DrinkDb_Auth.ViewModel.Authentication;
using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
using Microsoft.UI.Xaml;
using OtpNet;
using Repository.AdminDashboard;
using DrinkDb_Auth.ProxyRepository.AdminDashboard;

namespace DrinkDb_Auth.Service.Authentication
{
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
        public (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) Get2FAValues(IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null)
        {
            currentUser = userRepository?.GetUserById(userId).Result ?? throw new ArgumentException("User not found.");

            int keyLength = 42;
            byte[] twoFactorSecret;
            string uniformResourceIdentifier = "";


            if (isFirstTimeSetup)
            {
                twoFactorSecret = keyGeneration?.GenerateRandomKey(keyLength) ?? throw new InvalidOperationException("Failed to generate 2FA secret.");
                currentUser.TwoFASecret = Convert.ToBase64String(twoFactorSecret);
                userRepository.UpdateUser(currentUser);
                uniformResourceIdentifier = new OtpUri(OtpType.Totp, twoFactorSecret, currentUser.Username, "DrinkDB").ToString();
            }
            else
            {
                twoFactorSecret = Convert.FromBase64String(currentUser.TwoFASecret ?? string.Empty);
                Totp? timeBasedOneTimePassword = new Totp(twoFactorSecret);
            }

            return (currentUser, uniformResourceIdentifier, twoFactorSecret);
        }

        public static TwoFactorAuthenticationService CreateInjectedInstance(Window? window, Guid userId, bool isFirstTimeSetup, IUserRepository? userRepository = null, IKeyGeneration? keyGeneration = null, IVerify? verifier = null, IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null)
        {
            TwoFactorAuthenticationService instance = new TwoFactorAuthenticationService(window, userId, isFirstTimeSetup);
            instance.userRepository = userRepository;
            instance.keyGeneration = keyGeneration;
            instance.Get2FAValues(windowSetup, authenticationWindow, dialog, invalidDialog);
            return instance;
        }
    }
}