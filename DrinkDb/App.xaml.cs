// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace DrinkDb_Auth
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using DrinkDb_Auth.AutoChecker;
    using DrinkDb_Auth.Converters;
    using DrinkDb_Auth.Repository.AdminDashboard;
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
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static Guid CurrentUserId { get; set; } = Guid.Empty;
        public static Guid CurrentSessionId { get; set; } = Guid.Empty;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
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
                    string connectionString = config.GetConnectionString("DefaultConnection");
                    services.AddSingleton<IUserRepository, UserRepository>();
                    services.AddSingleton<IReviewsRepository, ReviewsRepository>(provider =>
                    {
                        ReviewsRepository repository = new ReviewsRepository();
                        return repository;
                    });
                    services.AddSingleton<IOffensiveWordsRepository>(provider =>
                    {
                        return new OffensiveWordsRepository(new SqlConnectionFactory(connectionString));
                    });
                    services.AddSingleton<IAutoCheck, AutoCheck>();
                    services.AddSingleton<ICheckersService, CheckersService>();
                    services.AddSingleton<IUpgradeRequestsRepository, UpgradeRequestsRepository>(provider =>
                    {
                        return new UpgradeRequestsRepository(new SqlConnectionFactory(connectionString));
                    });
                    services.AddSingleton<IRolesRepository, RolesRepository>();
                    services.AddSingleton<IUserService, UserService>();
                    services.AddSingleton<IReviewService, ReviewsService>();
                    services.AddSingleton<IUpgradeRequestsService, UpgradeRequestsService>();
                    services.AddTransient<EmailJob>();

                    IUserService userService = services.BuildServiceProvider().GetRequiredService<IUserService>();
                    UserIdToNameConverter.Initialize(userService);

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