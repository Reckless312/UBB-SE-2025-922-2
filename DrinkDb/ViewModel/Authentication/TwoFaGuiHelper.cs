using DataAccess.Model.Authentication;
using DrinkDb_Auth.ProxyRepository.AdminDashboard;
using DrinkDb_Auth.Service.Authentication.Components;
using DrinkDb_Auth.Service.Authentication.Interfaces;
using DrinkDb_Auth.View;
using DrinkDb_Auth.View.Authentication;
using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel.AdminDashboard.Components;
using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
using IRepository;
using Microsoft.UI.Xaml;
using Repository.AdminDashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrinkDb_Auth.ViewModel.Authentication
{
    internal class TwoFaGuiHelper
    {
        private IAuthenticationWindowSetup? windowSetup;
        private ITwoFactorAuthenticationView? authenticationWindow;
        private IDialog? dialog;
        private IDialog? invalidDialog;
        private Window? window;
        private RelayCommand? submitRelayCommand;
        private RelayCommand cancelRelayCommand;
        private IUserRepository? userRepository = new UserProxyRepository();
        private IVerify? twoFactorSecretVerifier = new Verify2FactorAuthenticationSecret();

        private TaskCompletionSource<bool> authentificationTask;
        private TaskCompletionSource<bool> authentificationCompleteTask;

        public TwoFaGuiHelper(Window? window)
        {
            this.window = window;
            authentificationTask = new TaskCompletionSource<bool>();
            authentificationCompleteTask = new TaskCompletionSource<bool>();
            cancelRelayCommand = new RelayCommand(() => { authentificationCompleteTask.SetResult(false); });
        }

        public void InitializeOtherComponents(bool firstTimeSetup, User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret)
        {
            if (firstTimeSetup)
            {
                this.windowSetup = new AuthenticationQRCodeAndTextBoxDigits(uniformResourceIdentifier);
                this.authenticationWindow = new TwoFactorAuthSetupView(this.windowSetup);
            }
            else
            {
                this.windowSetup = new AuthenticationQRCodeAndTextBoxDigits();
                this.authenticationWindow = new TwoFactorAuthCheckView(this.windowSetup);
            }

            submitRelayCommand = CreateSubmitRelayCommand(this.windowSetup, currentUser, twoFactorSecret, authentificationTask, firstTimeSetup);
            this.dialog = CreateAuthentificationSubWindow(window, this.authenticationWindow, submitRelayCommand, dialog);
            this.invalidDialog = invalidDialog == null ? new InvalidAuthenticationWindow("Error", "Invalid 2FA code. Please try again.", "OK", cancelRelayCommand, window) : invalidDialog;
            this.invalidDialog.CreateContentDialog();
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
                                bool result = userRepository?.UpdateUser(user).Result ?? false;
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

        public async Task<bool> SetupOrVerifyTwoFactor()
        {
            dialog?.ShowAsync();
            bool authentificationResult = await authentificationTask.Task;

            authentificationCompleteTask = new TaskCompletionSource<bool>();
            dialog?.Hide();

            ShowResults(window, authentificationCompleteTask, authentificationResult);
            return await authentificationCompleteTask.Task;
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
