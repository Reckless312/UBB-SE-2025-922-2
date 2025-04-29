using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
using Microsoft.UI.Xaml;

namespace DrinkDb_Auth.Service.Authentication.Interfaces
{
    internal interface ITwoFactorAuthenticationService
    {
        Task<bool> SetupOrVerifyTwoFactor();
        void InitializeOtherComponents(IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null);
    }
}
