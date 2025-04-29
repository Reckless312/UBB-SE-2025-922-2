// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using App1.AutoChecker;
    using App1.Converters;
    using App1.Infrastructure;
    using App1.Models;
    using App1.Repositories;
    using App1.Services;
    using App1.Views;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.UI.Xaml;
    using Quartz;
    using Quartz.Impl;

    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.ConfigureHost();
        }

        public static IHost Host { get; private set; }

        public static Window MainWindow { get; set; }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
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
                        repository.LoadReviews(ReviewsSampleData.GetSampleReviews());
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

        public static class ReviewsSampleData
        {
            public static IEnumerable<Review> GetSampleReviews()
            {
                return new List<Review>
                {
                    new Review(
                        reviewId: 0,
                        userId: 1,
                        rating: 5,
                        content: "Terrible mix, a complete mess",
                        createdDate: DateTime.Now.AddHours(-1),
                        numberOfFlags: 1,
                        isHidden: false),
                    new Review(
                        reviewId: 0,
                        userId: 3,
                        rating: 4,
                        content: "Good experience",
                        createdDate: DateTime.Now.AddHours(-5),
                        isHidden: false),
                    new Review(
                        reviewId: 0,
                        userId: 1,
                        rating: 2,
                        content: "Such a bitter aftertaste",
                        createdDate: DateTime.Now.AddDays(-1),
                        numberOfFlags: 3,
                        isHidden: false),
                    new Review(
                        reviewId: 0,
                        userId: 5,
                        rating: 5,
                        content: "Excellent!",
                        createdDate: DateTime.Now.AddDays(-2),
                        numberOfFlags: 1,
                        isHidden: false),
                    new Review(
                        reviewId: 0,
                        userId: 3,
                        rating: 5,
                        content: "dunce",
                        createdDate: DateTime.Now.AddDays(-2),
                        numberOfFlags: 1,
                        isHidden: false),
                    new Review(
                        reviewId: 0,
                        userId: 5,
                        rating: 5,
                        content: "Amazing",
                        createdDate: DateTime.Now.AddDays(-2),
                        isHidden: false),
                    new Review(
                        reviewId: 0,
                        userId: 1,
                        rating: 5,
                        content: "My favorite!",
                        createdDate: DateTime.Now.AddDays(-2),
                        isHidden: false),
                };
            }
        }
    }
}
