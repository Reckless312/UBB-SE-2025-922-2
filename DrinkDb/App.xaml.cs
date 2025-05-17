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
    using DrinkDb_Auth.Converters;
    using DrinkDb_Auth.ProxyRepository.AdminDashboard;
    using DrinkDb_Auth.ProxyRepository.AutoChecker;
    using DrinkDb_Auth.Service;
    using DrinkDb_Auth.Service.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
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
                    IConfiguration config = new ConfigurationBuilder().AddUserSecrets<App>().AddEnvironmentVariables().AddJsonFile("appSettings.json", optional: false, reloadOnChange: true).Build();
                    services.AddSingleton<IConfiguration>(config);
                    string connectionString = config.GetConnectionString("DrinkDbConnection");
                    string apiRoute = "http://localhost:5280/";
                    services.AddHttpClient<IUserRepository, UserProxyRepository>(provider =>
                    {
                        UserProxyRepository repository = new UserProxyRepository(apiRoute);
                        return repository;
                    });
                    services.AddSingleton<IReviewsRepository, ReviewsProxyRepository>(provider =>
                    {
                        ReviewsProxyRepository repository = new ReviewsProxyRepository(apiRoute);
                        return repository;
                    });
                    services.AddSingleton<IOffensiveWordsRepository, OffensiveWordsProxyRepository>(provider =>
                    {
                        OffensiveWordsProxyRepository repository = new OffensiveWordsProxyRepository(apiRoute);
                        return repository;
                    });
                    services.AddSingleton<IAutoCheck, AutoCheck>();
                    services.AddSingleton<ICheckersService, CheckersService>();
                    services.AddSingleton<IUpgradeRequestsRepository, UpgradeRequestProxyRepository>(provider =>
                    {
                        return new UpgradeRequestProxyRepository(apiRoute);
                    });
                    services.AddSingleton<IRolesRepository, RolesProxyRepository>(provider =>
                    {
                        return new RolesProxyRepository(apiRoute);
                    });
                    services.AddSingleton<IUserService, UserService>();
                    services.AddSingleton<IReviewService, ReviewsService>();
                    services.AddSingleton<IUpgradeRequestsService, UpgradeRequestsService>();

                    // services.AddSingleton<IAuthenticationService, AuthenticationService>(); PROXY NEEDED HERE
                    services.AddTransient<EmailJob>();

                    // BLOWS UP HERE
                    // IUserService userService = services.BuildServiceProvider().GetRequiredService<IUserService>();
                    // UserIdToNameConverter.Initialize(userService);

                    // Quartz Configuration
                    services.AddSingleton<JobFactory>();
                    services.AddSingleton(provider =>
                    {
                        StdSchedulerFactory factory = new StdSchedulerFactory();
                        IScheduler scheduler = factory.GetScheduler().Result;
                        scheduler.JobFactory = provider.GetRequiredService<JobFactory>();
                        return scheduler;
                    });

                    // Jobs
                    services.AddTransient<EmailJob>();
                    services.AddTransient<MainPage>();
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }
    }
}