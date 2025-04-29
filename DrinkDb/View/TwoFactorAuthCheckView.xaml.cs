using DrinkDb_Auth.View.Authentication.Interfaces;
using DrinkDb_Auth.ViewModel.Authentication;
using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace DrinkDb_Auth.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TwoFactorAuthCheckView : Page, ITwoFactorAuthenticationView
    {
        public TwoFactorAuthCheckView(IAuthenticationWindowSetup authentificationHandler)
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
