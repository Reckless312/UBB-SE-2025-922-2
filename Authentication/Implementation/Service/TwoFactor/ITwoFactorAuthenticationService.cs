using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrinkDb_Auth.View;
using DrinkDb_Auth.ViewModel;
using Microsoft.UI.Xaml;

namespace DrinkDb_Auth.Service.TwoFactor
{
    internal interface ITwoFactorAuthenticationService
    {
        Task<bool> SetupOrVerifyTwoFactor();
        void InitializeOtherComponents(IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null);
    }
}
