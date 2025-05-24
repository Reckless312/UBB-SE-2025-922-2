using System;
using DataAccess.AuthProviders;
using DataAccess.AuthProviders.Facebook;
using DataAccess.AuthProviders.Github;
using DataAccess.AuthProviders.LinkedIn;
using DataAccess.AuthProviders.Twitter;
using DataAccess.AutoChecker;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication.Interfaces;
using DrinkDb_Auth.AuthProviders.Google;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.ServiceProxy;
using DrinkDb_Auth.ServiceProxy.AdminDashboard;
using DrinkDb_Auth.ServiceProxy.Authentication;
using DrinkDb_Auth.View;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Quartz;
using Quartz.Impl;

namespace DrinkDb_Auth
{
    sealed partial class App : Application
    {
        public static Guid CurrentUserId { get; set; } = Guid.Empty;
        public static Guid CurrentSessionId { get; set; } = Guid.Empty;

        public App()
        {
            this.InitializeComponent();
            this.ConfigureHost();
        }

        public static IHost Host { get; private set; }

        public static Window MainWindow { get; set; }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs e)
        {
            IScheduler scheduler = Host.Services.GetRequiredService<IScheduler>();
            scheduler.Start().Wait();

            MainWindow = Host.Services.GetRequiredService<MainWindow>();
            MainWindow.Activate();

            // Prevent app suspension
            Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += (s, e) =>
            {
                MainWindow?.Activate();
            };
        }

        private void ConfigureHost()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    IConfiguration config = new ConfigurationBuilder()
                        .AddUserSecrets<App>()
                        .AddEnvironmentVariables()
                        .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                        .Build();

                    services.AddSingleton<IConfiguration>(config);

                    string apiRoute = "http://localhost:5280/";

                    services.AddSingleton<ISessionService, SessionServiceProxy>(sp => new SessionServiceProxy(apiRoute));
                    services.AddSingleton<IAuthenticationService>(sp => new AuthenticationServiceProxy(apiRoute));
                    services.AddSingleton<IUserService>(sp => new UserServiceProxy(apiRoute));
                    services.AddSingleton<ICheckersService>(sp => new OffensiveWordsServiceProxy(apiRoute));
                    services.AddSingleton<IReviewService>(sp => new ReviewsServiceProxy(apiRoute));
                    services.AddSingleton<IUpgradeRequestsService>(sp => new UpgradeRequestsServiceProxy(apiRoute));
                    services.AddSingleton<IRolesService, RolesProxyService>(sp => new RolesProxyService(apiRoute));
                    services.AddSingleton<IAutoCheck, AutoCheckerProxy>(sp => new AutoCheckerProxy(apiRoute));
                    services.AddSingleton<IBasicAuthenticationProvider>(sp => new BasicAuthenticationProviderServiceProxy(apiRoute));
                    services.AddSingleton<ITwoFactorAuthenticationService>(sp => new TwoFactorAuthenticationServiceProxy(apiRoute));

                    services.AddSingleton<LinkedInLocalOAuthServer>(sp => new LinkedInLocalOAuthServer("http://localhost:8891/"));
                    services.AddSingleton<GitHubLocalOAuthServer>(sp => new GitHubLocalOAuthServer("http://localhost:8890/"));
                    services.AddSingleton<FacebookLocalOAuthServer>(sp => new FacebookLocalOAuthServer("http://localhost:8888/"));

                    services.AddSingleton<IGitHubHttpHelper, GitHubHttpHelper>();
                    services.AddSingleton<GitHubOAuth2Provider>(sp =>
                        new GitHubOAuth2Provider(
                            sp.GetRequiredService<IUserService>(),
                            sp.GetRequiredService<ISessionService>()));
                    services.AddSingleton<IGitHubOAuthHelper>(sp =>
                        new GitHubOAuthHelper(
                            sp.GetRequiredService<GitHubOAuth2Provider>(),
                            sp.GetRequiredService<GitHubLocalOAuthServer>()));
                    services.AddSingleton<IGoogleOAuth2Provider, GoogleOAuth2Provider>();
                    services.AddSingleton<FacebookOAuth2Provider>(sp =>
                        new FacebookOAuth2Provider(
                            sp.GetRequiredService<ISessionService>(),
                            sp.GetRequiredService<IUserService>()));
                    services.AddSingleton<IFacebookOAuthHelper, FacebookOAuthHelper>();
                    services.AddSingleton<LinkedInOAuth2Provider>(sp =>
                        new LinkedInOAuth2Provider(
                            sp.GetRequiredService<IUserService>(),
                            sp.GetRequiredService<ISessionService>()));
                    services.AddSingleton<ILinkedInOAuthHelper>(sp => new LinkedInOAuthHelper(
                        "86j0ikb93jm78x",
                        "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==",
                        "http://localhost:8891/auth",
                        "openid profile email",
                        sp.GetRequiredService<LinkedInOAuth2Provider>()));
                    services.AddSingleton<TwitterOAuth2Provider>(sp =>
                        new TwitterOAuth2Provider(
                            sp.GetRequiredService<IUserService>(),
                            sp.GetRequiredService<ISessionService>()));

                    // Quartz Configuration
                    services.AddSingleton<JobFactory>();
                    services.AddSingleton(provider =>
                    {
                        StdSchedulerFactory factory = new StdSchedulerFactory();
                        IScheduler scheduler = factory.GetScheduler().Result;
                        scheduler.JobFactory = provider.GetRequiredService<JobFactory>();
                        return scheduler;
                    });

                    services.AddTransient<MainPage>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<UserPage>();
                })
                .Build();
        }
    }
}