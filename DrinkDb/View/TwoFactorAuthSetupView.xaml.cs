using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel.Authentication;
using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace DrinkDb_Auth.View
{
    public sealed partial class TwoFactorAuthSetupView : Page, ITwoFactorAuthenticationView
    {
        public TwoFactorAuthSetupView(IAuthenticationWindowSetup authentificationHandler)
        {
            this.InitializeComponent();
            this.DataContext = authentificationHandler;
        }

        public void TextBox_KeyUp(object sender, KeyRoutedEventArgs @event)
        {
            AuthenticationTextBox.TextBoxKeyEvent(sender, @event, this);
        }
    }
}
