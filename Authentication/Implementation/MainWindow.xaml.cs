using System;
using System.Threading.Tasks;
using DrinkDb_Auth.OAuthProviders;
using DrinkDb_Auth.Service;
using DrinkDb_Auth.AuthProviders.Facebook;
using DrinkDb_Auth.AuthProviders.Github;
using DrinkDb_Auth.AuthProviders.Google;
using DrinkDb_Auth.AuthProviders.LinkedIn;
using DrinkDb_Auth.AuthProviders.Twitter;
using DrinkDb_Auth.Service.TwoFactor;
using Microsoft.IdentityModel.Tokens;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace DrinkDb_Auth
{
    public sealed partial class MainWindow : Window
    {
        private AuthenticationService authenticationService = new ();
        private ITwoFactorAuthenticationService? twoFactorAuthentificationService;

        public MainWindow()
        {
            this.InitializeComponent();

            Title = "DrinkDb - Sign In";

            this.AppWindow.Resize(new SizeInt32
            {
                Width = DisplayArea.Primary.WorkArea.Width,
                Height = DisplayArea.Primary.WorkArea.Height
            });
            this.AppWindow.Move(new PointInt32(0, 0));
        }

        private async Task<bool> AuthenticationComplete(AuthenticationResponse res)
        {
            if (res.AuthenticationSuccessful)
            {
                var user = authenticationService.GetUser(res.SessionId);
                bool twoFAresponse = false;
                if (!user.TwoFASecret.IsNullOrEmpty())
                {
                    this.twoFactorAuthentificationService = new TwoFactorAuthenticationService(this, user.UserId, false);
                    this.twoFactorAuthentificationService.InitializeOtherComponents();
                    twoFAresponse = await this.twoFactorAuthentificationService.SetupOrVerifyTwoFactor();
                }
                else
                {
                    this.twoFactorAuthentificationService = new TwoFactorAuthenticationService(this, user.UserId, true);
                    this.twoFactorAuthentificationService.InitializeOtherComponents();
                    twoFAresponse = await this.twoFactorAuthentificationService.SetupOrVerifyTwoFactor();
                }

                if (twoFAresponse)
                {
                    App.CurrentUserId = user.UserId;
                    App.CurrentSessionId = res.SessionId;
                    MainFrame.Navigate(typeof(SuccessPage), this);
                    return true;
                }
                return false;
            }
            else
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Authentication Failed",
                    Content = "Authentication was not successful. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = Content.XamlRoot
                };
                _ = errorDialog.ShowAsync();
            }
            return false;
        }

        public void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            AuthenticationResponse response = authenticationService.AuthWithUserPass(username, password);
            _ = AuthenticationComplete(response);
        }

        public async void GithubSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authResponse = await authenticationService.AuthWithOAuth(this, OAuthService.GitHub, new GitHubOAuthHelper());
                _ = AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await ShowError("Authentication Error", ex.ToString());
            }
        }

        public async void GoogleSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GoogleSignInButton.IsEnabled = false;
                var authResponse = await authenticationService.AuthWithOAuth(this, OAuthService.Google, new GoogleOAuth2Provider());
                await AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await ShowError("Error", ex.Message);
            }
            finally
            {
                GoogleSignInButton.IsEnabled = true;
            }
        }

        public async void FacebookSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authResponse = await authenticationService.AuthWithOAuth(this, OAuthService.Facebook, new FacebookOAuthHelper());
                await AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await ShowError("Authentication Error", ex.ToString());
            }
        }

        public async void XSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                XSignInButton.IsEnabled = false;
                var authResponse = await authenticationService.AuthWithOAuth(this, OAuthService.Twitter, new TwitterOAuth2Provider());
                await AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await ShowError("Error", ex.Message);
            }
            finally
            {
                XSignInButton.IsEnabled = true;
            }
        }

        public async void LinkedInSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authResponse = await authenticationService.AuthWithOAuth(this, OAuthService.LinkedIn, new LinkedInOAuthHelper(
                    clientId: "86j0ikb93jm78x",
                    clientSecret: "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==",
                    redirectUri: "http://localhost:8891/auth",
                    scope: "openid profile email"));
                await AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await ShowError("Authentication Error", ex.ToString());
            }
        }

        private async Task ShowError(string title, string content)
        {
            var errorDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
