namespace DrinkDb_Auth
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using DataAccess.AutoChecker;
    using DataAccess.Service;
    using DataAccess.Service.AdminDashboard;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using DataAccess.Service.Authentication;
    using DataAccess.Service.Authentication.Interfaces;

    using DrinkDb_Auth.ServiceProxy;
    using DrinkDb_Auth.Converters;
    using DrinkDb_Auth.ProxyRepository.AdminDashboard;
    using DrinkDb_Auth.ProxyRepository.AutoChecker;
    using DrinkDb_Auth.Service;
    using DrinkDb_Auth.Service.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using DrinkDb_Auth.ServiceProxy;
    using DrinkDb_Auth.View;
    using IRepository;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Quartz;
    using Quartz.Impl;
    using Quartz.Spi;
    using Repository.AdminDashboard;
    using ServerAPI.Repository.AutoChecker;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.Media.Protection.PlayReady;
    using DataAccess.AuthProviders;
    using DataAccess.AuthProviders.Facebook;
    using DataAccess.AuthProviders.Github;
    using DataAccess.AuthProviders.LinkedIn;
    using DrinkDb_Auth.ServiceProxy;
    using System.Net.Http;
    using DrinkDb_Auth.Service.Authentication;
    using Repository.Authentication;
    using ServerAPI.Data;
    using Microsoft.EntityFrameworkCore;
    using DrinkDb_Auth.ProxyRepository.Authentification;
    using DrinkDb_Auth.AuthProviders.Google;
    using DrinkDb_Auth.ServerProxy;

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

                    // Configure HttpClient
                    services.AddHttpClient("DrinkDbClient", client =>
                    {
                        client.BaseAddress = new Uri(apiRoute);
                    });

                    // Register Proxy Services
                    services.AddSingleton<ISessionService, SessionServiceProxy>();
                    services.AddSingleton<IAuthenticationService>(sp => 
                        new AuthenticationServiceProxy(
                            sp.GetRequiredService<IHttpClientFactory>().CreateClient("DrinkDbClient")));
                    services.AddSingleton<IUserService>(sp =>
                        new UserServiceProxy("http://localhost:5280/"));
                    services.AddSingleton<ICheckersService>(sp =>
                    new OffensiveWordsServiceProxy(
                        sp.GetRequiredService<IHttpClientFactory>().CreateClient("DrinkDbClient"),
                        "http://localhost:5280/"));

                    services.AddSingleton<IReviewService>(sp =>
                    new ReviewsServiceProxy(
                        sp.GetRequiredService<IHttpClientFactory>().CreateClient("DrinkDbClient"),
                        "http://localhost:5280/"));

                    services.AddSingleton<IUpgradeRequestsService>(sp =>
                    new UpgradeRequestsServiceProxy(
                        sp.GetRequiredService<IHttpClientFactory>().CreateClient("DrinkDbClient"),
                        "http://localhost:5280/"
                    ));

                    services.AddSingleton<IRolesService, RolesProxyService>();

                    // Register Original Services
                    // services.AddSingleton<ISessionService, SessionService>();
                    //services.AddSingleton<IAuthenticationService>(sp => new AuthenticationService(
                    //    sp.GetRequiredService<ISessionRepository>(),
                    //    sp.GetRequiredService<IUserRepository>(),
                    //    sp.GetRequiredService<LinkedInLocalOAuthServer>(),
                    //    sp.GetRequiredService<GitHubLocalOAuthServer>(),
                    //    sp.GetRequiredService<FacebookLocalOAuthServer>(),
                    //    sp.GetRequiredService<IBasicAuthenticationProvider>()));
                    //services.AddSingleton<IUserService, UserService>();
                    //services.AddSingleton<IReviewService, ReviewsService>();

                    //services.AddSingleton<IUpgradeRequestsService, UpgradeRequestsService>();

                    // Register Repositories
                    services.AddSingleton<ISessionRepository, SessionProxyRepository>();
                    services.AddSingleton<IUserRepository, UserProxyRepository>();
                    services.AddSingleton<IReviewsRepository>(sp =>
                        new ReviewsProxyRepository("http://localhost:5280/"));
                    services.AddSingleton<IOffensiveWordsRepository, OffensiveWordsProxyRepository>();
                    services.AddSingleton<IUpgradeRequestsRepository>(sp =>
                       new UpgradeRequestProxyRepository("http://localhost:5280/"));
                    services.AddSingleton<IRolesRepository>(sp =>
                        new RolesProxyRepository("http://localhost:5280/"));

                    // Register OAuth Servers
                    services.AddSingleton<LinkedInLocalOAuthServer>(sp =>
                        new LinkedInLocalOAuthServer("http://localhost:8891/"));
                    services.AddSingleton<GitHubLocalOAuthServer>(sp =>
                        new GitHubLocalOAuthServer("http://localhost:8890/"));
                    services.AddSingleton<FacebookLocalOAuthServer>(sp =>
                        new FacebookLocalOAuthServer("http://localhost:8888/"));

                    // Register OAuth Helpers
                    services.AddSingleton<IGitHubHttpHelper, GitHubHttpHelper>();
                    services.AddSingleton<GitHubOAuth2Provider>(sp =>
                        new GitHubOAuth2Provider(
                            sp.GetRequiredService<IUserRepository>(),
                            sp.GetRequiredService<ISessionRepository>(),
                            sp.GetRequiredService<IGitHubHttpHelper>()
                        ));
                    services.AddSingleton<IGitHubOAuthHelper>(sp =>
                        new GitHubOAuthHelper(
                            sp.GetRequiredService<GitHubOAuth2Provider>(),
                            sp.GetRequiredService<GitHubLocalOAuthServer>()
                        ));
                    services.AddSingleton<IGoogleOAuth2Provider, GoogleOAuth2Provider>();
                    services.AddSingleton<IFacebookOAuthHelper, FacebookOAuthHelper>();
                    services.AddSingleton<ILinkedInOAuthHelper>(sp => new LinkedInOAuthHelper(
                        "86j0ikb93jm78x",
                        "WPL_AP1.pg2Bd1XhCi821VTG.+hatTA==",
                        "http://localhost:8891/auth",
                        "openid profile email"
                    ));

                    // Register Services
                    services.AddSingleton<IAutoCheck, AutoCheck>();
                    //services.AddSingleton<ICheckersService, CheckersService>();
                    services.AddSingleton<IBasicAuthenticationProvider>(sp =>
                        new BasicAuthenticationProviderServiceProxy(sp.GetRequiredService<IHttpClientFactory>().CreateClient("DrinkDbClient")));
                    services.AddTransient<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();

                    // Quartz Configuration
                    services.AddSingleton<JobFactory>();
                    services.AddSingleton(provider =>
                    {
                        StdSchedulerFactory factory = new StdSchedulerFactory();
                        IScheduler scheduler = factory.GetScheduler().Result;
                        scheduler.JobFactory = provider.GetRequiredService<JobFactory>();
                        return scheduler;
                    });

                    // Jobs and UI Components
                    services.AddTransient<EmailJob>();
                    services.AddTransient<MainPage>();
                    services.AddTransient<MainWindow>();
                    services.AddTransient<UserPage>();
                })
                .Build();
        }
    }
}