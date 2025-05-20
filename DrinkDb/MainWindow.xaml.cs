namespace DrinkDb_Auth
{
    using System;
    using System.Threading.Tasks;
    using DataAccess.AuthProviders.Facebook;
    using DataAccess.AuthProviders.Github;
    using DataAccess.AuthProviders.LinkedIn;
    using DataAccess.AuthProviders.Twitter;
    using DataAccess.Model.Authentication;
    using DataAccess.OAuthProviders;
    using DataAccess.Service.Authentication;
    using DataAccess.Service.Authentication.Interfaces;
    using DrinkDb_Auth.AuthProviders.Google;
    using DrinkDb_Auth.Service.Authentication;
    using DrinkDb_Auth.ViewModel.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.UI;
    using Microsoft.UI.Windowing;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Quartz;
    using Quartz.Impl;
    using Windows.Graphics;

    public sealed partial class MainWindow : Window
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ITwoFactorAuthenticationService twoFactorAuthentificationService;
        private readonly IScheduler scheduler;

        public Frame NavigationFrame { get; private set; }

        public MainWindow(IAuthenticationService authenticationService, IScheduler scheduler, ITwoFactorAuthenticationService twoFactorAuthenticationService)
        {
            this.authenticationService = authenticationService;
            this.scheduler = scheduler;
            this.twoFactorAuthentificationService = twoFactorAuthenticationService;
            this.InitializeComponent();
            this.NavigationFrame = this.MainFrame;

            this.Title = "DrinkDb - Sign In";

            this.AppWindow.Resize(new SizeInt32
            {
                Width = DisplayArea.Primary.WorkArea.Width,
                Height = DisplayArea.Primary.WorkArea.Height,
            });
            this.AppWindow.Move(new PointInt32(0, 0));

            this.ScheduleDelayedEmailAutomatically().ConfigureAwait(false);

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }
        }

        private async Task InitializeScheduler()
        {
            try
            {
                StdSchedulerFactory factory = new StdSchedulerFactory();
                await this.scheduler.Start();
                System.Diagnostics.Debug.WriteLine("Scheduler initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Scheduler initialization failed: {ex}");
            }
        }

        private async Task ScheduleDelayedEmailAutomatically()
        {
            try
            {
                IJobDetail job = JobBuilder.Create<EmailJob>()
                    .WithIdentity("autoEmailJob", "emailGroup")
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("autoTrigger", "emailGroup")
                    .StartNow()
                    .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Monday, 11, 40))
                    .Build();

                await this.scheduler.ScheduleJob(job, trigger);
                System.Diagnostics.Debug.WriteLine($"Job scheduled to run every 1 minute");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Job scheduling failed: {ex}");
            }
        }

        private async Task<bool> AuthenticationComplete(AuthenticationResponse res)
        {
            if (res.AuthenticationSuccessful)
            {
                User user = await this.authenticationService.GetUser(res.SessionId);
                bool twoFAresponse = false;
                bool firstTimeSetup = user.TwoFASecret.IsNullOrEmpty();
                this.twoFactorAuthentificationService.UserId = user.UserId;
                this.twoFactorAuthentificationService.IsFirstTimeSetup = firstTimeSetup;
                TwoFaGuiHelper twoFaGuiHelper = new TwoFaGuiHelper(this);
                (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret)
                    values = this.twoFactorAuthentificationService.Get2FAValues();
                twoFaGuiHelper.InitializeOtherComponents(firstTimeSetup, values.currentUser, values.uniformResourceIdentifier, values.twoFactorSecret);
                twoFAresponse = await twoFaGuiHelper.SetupOrVerifyTwoFactor();

                if (twoFAresponse)
                {
                    App.CurrentUserId = user.UserId;
                    App.CurrentSessionId = res.SessionId;
                    this.NavigationFrame.Navigate(typeof(SuccessPage), this);
                }

                return twoFAresponse;
            }
            else
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Authentication Failed",
                    Content = "Authentication was not successful. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };
                await errorDialog.ShowAsync();
            }

            return false;
        }

        public async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = this.UsernameTextBox.Text;
                string password = this.PasswordBox.Password;

                AuthenticationResponse response = await this.authenticationService.AuthWithUserPass(username, password);
                await this.AuthenticationComplete(response);
            }
            catch (Exception ex)
            {
                await this.ShowError("Authentication Error", ex.ToString());
            }
        }

        public async void GithubSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var githubHelper = App.Host.Services.GetRequiredService<IGitHubOAuthHelper>();
                AuthenticationResponse authResponse = null;
                try
                {
                    authResponse = await this.authenticationService.AuthWithOAuth(OAuthService.GitHub, githubHelper);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception in AuthWithOAuth: {ex}");
                    throw;
                }
                _ = this.AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await this.ShowError("Authentication Error", ex.ToString());
            }
        }

        public async void GoogleSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.GoogleSignInButton.IsEnabled = false;
                var googleHelper = App.Host.Services.GetRequiredService<IGoogleOAuth2Provider>();
                AuthenticationResponse authResponse = await this.authenticationService.AuthWithOAuth(OAuthService.Google, googleHelper);
                await this.AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await ShowError("Error", ex.Message);
            }
            finally
            {
                this.GoogleSignInButton.IsEnabled = true;
            }
        }

        public async void FacebookSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var facebookHelper = App.Host.Services.GetRequiredService<IFacebookOAuthHelper>();
                AuthenticationResponse authResponse = await this.authenticationService.AuthWithOAuth(OAuthService.Facebook, facebookHelper);
                await this.AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await this.ShowError("Authentication Error", ex.ToString());
            }
        }

        public async void XSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.XSignInButton.IsEnabled = false;
                AuthenticationResponse authResponse = await this.authenticationService.AuthWithOAuth(OAuthService.Twitter, new TwitterOAuth2Provider());
                await this.AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await this.ShowError("Error", ex.Message);
            }
            finally
            {
                this.XSignInButton.IsEnabled = true;
            }
        }

        public async void LinkedInSignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var linkedInHelper = App.Host.Services.GetRequiredService<ILinkedInOAuthHelper>();
                AuthenticationResponse authResponse = await this.authenticationService.AuthWithOAuth(OAuthService.LinkedIn, linkedInHelper);
                await this.AuthenticationComplete(authResponse);
            }
            catch (Exception ex)
            {
                await this.ShowError("Authentication Error", ex.ToString());
            }
        }

        private async Task ShowError(string title, string content)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot,
            };
            await errorDialog.ShowAsync();
        }
    }
}
