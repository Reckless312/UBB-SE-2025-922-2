using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
using Microsoft.UI.Xaml;

namespace DrinkDb_Auth.Service.Authentication.Interfaces
{
    internal interface ITwoFactorAuthenticationService
    {
        (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) Get2FAValues(IAuthenticationWindowSetup? windowSetup = null, ITwoFactorAuthenticationView? authenticationWindow = null, IDialog? dialog = null, IDialog? invalidDialog = null);
    }
}
