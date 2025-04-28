using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombinedProject.View;
using CombinedProject.ViewModel;
using Microsoft.UI.Xaml;

namespace CombinedProject.Service.TwoFactor
{
    internal interface ITwoFactorAuthenticationService
    {
        Task<bool> SetupOrVerifyTwoFactor();
        void InitializeOtherComponents(IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null);
    }
}
