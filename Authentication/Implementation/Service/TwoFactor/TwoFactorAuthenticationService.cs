using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.View;
using DrinkDb_Auth.ViewModel;
using Microsoft.UI.Xaml;
using OtpNet;

namespace DrinkDb_Auth.Service.TwoFactor
{
    public class TwoFactorAuthenticationService : ITwoFactorAuthenticationService
    {
        private IUserAdapter? userDatabaseAdapter = new UserAdapter();
        private IKeyGeneration? keyGeneration = new OtpKeyGeneration();
        private IVerify? twoFactorSecretVerifier = new Verify2FactorAuthenticationSecret();
        private IAuthenticationWindowSetup? windowSetup;
        private ITwoFactorAuthenticationView? authenticationWindow;
        private IDialog? dialog;
        private IDialog? invalidDialog;

        private Window? window;
        private TaskCompletionSource<bool> authentificationTask;
        private TaskCompletionSource<bool> authentificationCompleteTask;
        private User? currentUser;
        private RelayCommand? submitRelayCommand;
        private RelayCommand cancelRelayCommand;

        private Guid userId;
        private bool isFirstTimeSetup;

        public TwoFactorAuthenticationService(Window? window, Guid userId, bool isFirstTimeSetup)
        {
            this.window = window;
            this.userId = userId;
            this.isFirstTimeSetup = isFirstTimeSetup;

            authentificationTask = new TaskCompletionSource<bool>();
            authentificationCompleteTask = new TaskCompletionSource<bool>();

            cancelRelayCommand = new RelayCommand(() => { authentificationCompleteTask.SetResult(false); });
        }
        public void InitializeOtherComponents(IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null)
        {
            currentUser = userDatabaseAdapter?.GetUserById(userId) ?? throw new ArgumentException("User not found.");

            int keyLength = 42;
            byte[] twoFactorSecret;

            switch (isFirstTimeSetup)
            {
                case true:
                    twoFactorSecret = keyGeneration?.GenerateRandomKey(keyLength) ?? throw new InvalidOperationException("Failed to generate 2FA secret.");
                    currentUser.TwoFASecret = Convert.ToBase64String(twoFactorSecret);
                    string? uniformResourceIdentifier = new OtpUri(OtpType.Totp, twoFactorSecret, currentUser.Username, "DrinkDB").ToString();
                    this.windowSetup = windowSetup == null ? new AuthenticationQRCodeAndTextBoxDigits(uniformResourceIdentifier) : windowSetup;
                    this.authenticationWindow = authenticationWindow == null ? new TwoFactorAuthSetupView(this.windowSetup) : authenticationWindow;
                    break;
                case false:
                    twoFactorSecret = Convert.FromBase64String(currentUser.TwoFASecret ?? string.Empty);
                    Totp? timeBasedOneTimePassword = new Totp(twoFactorSecret);
                    this.windowSetup = windowSetup == null ? new AuthenticationQRCodeAndTextBoxDigits() : windowSetup;
                    this.authenticationWindow = authenticationWindow == null ? new TwoFactorAuthCheckView(this.windowSetup) : authenticationWindow;
                    break;
            }

            submitRelayCommand = CreateSubmitRelayCommand(this.windowSetup, currentUser, twoFactorSecret, authentificationTask, isFirstTimeSetup);
            this.dialog = CreateAuthentificationSubWindow(window, this.authenticationWindow, submitRelayCommand, dialog);
            this.invalidDialog = invalidDialog == null ? new InvalidAuthenticationWindow("Error", "Invalid 2FA code. Please try again.", "OK", cancelRelayCommand, window) : invalidDialog;
            this.invalidDialog.CreateContentDialog();
        }

        public static TwoFactorAuthenticationService CreateInjectedInstance(Window? window, Guid userId, bool isFirstTimeSetup, IUserAdapter? userAdapter = null, IKeyGeneration? keyGeneration = null, IVerify? verifier = null, IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null)
        {
            TwoFactorAuthenticationService instance = new TwoFactorAuthenticationService(window, userId, isFirstTimeSetup);
            instance.userDatabaseAdapter = userAdapter;
            instance.keyGeneration = keyGeneration;
            instance.twoFactorSecretVerifier = verifier;
            instance.InitializeOtherComponents(windowSetup, authenticationWindow, dialog, invalidDialog);
            return instance;
        }

        public async Task<bool> SetupOrVerifyTwoFactor()
        {
            dialog?.ShowAsync();
            bool authentificationResult = await authentificationTask.Task;

            authentificationCompleteTask = new TaskCompletionSource<bool>();
            dialog?.Hide();

            ShowResults(window, authentificationCompleteTask, authentificationResult);
            return await authentificationCompleteTask.Task;
        }

        private IDialog CreateAuthentificationSubWindow(Window? window, object view, RelayCommand command, IDialog? dialog = null)
        {
            this.dialog = dialog == null ? new AuthenticationCodeWindow("Set up two factor auth", "Cancel", "Submit", command, window, view) : dialog;
            this.dialog.CreateContentDialog();
            this.dialog.Command = command;
            return this.dialog;
        }

        private RelayCommand CreateSubmitRelayCommand(IAuthenticationWindowSetup authentificationHandler, User user, byte[] twoFactorSecret, TaskCompletionSource<bool> codeSetupTask, bool updateDatabase)
        {
            return new RelayCommand(() =>
            {
                string providedCode = authentificationHandler.FirstDigit
                            + authentificationHandler.SecondDigit
                            + authentificationHandler.ThirdDigit
                            + authentificationHandler.FourthDigit
                            + authentificationHandler.FifthDigit
                            + authentificationHandler.SixthDigit;
                switch (twoFactorSecretVerifier?.Verify2FAForSecret(twoFactorSecret, providedCode))
                {
                    case true:
                        // Test updating database or not
                        switch (updateDatabase)
                        {
                            case true:
                                bool result = userDatabaseAdapter?.UpdateUser(user) ?? false;
                                // Test both branches of updating 2fa
                                if (!result)
                                {
                                    throw new InvalidOperationException("Failed to update user with 2FA secret.");
                                }
                                break;
                            case false:
                                break;
                        }
                        codeSetupTask.SetResult(true);
                        break;
                    case false:
                        codeSetupTask.SetResult(false);
                        break;
                }
            });
        }

        private void ShowResults(Window? window, TaskCompletionSource<bool> authCompletionStatus, bool codeSetupResult)
        {
            // Test both branches of results
            if (codeSetupResult)
            {
                authCompletionStatus.SetResult(true);
            }
            else
            {
                if (invalidDialog != null)
                {
                    invalidDialog.Command = cancelRelayCommand;
                }
                invalidDialog?.ShowAsync();
            }
        }
    }
}