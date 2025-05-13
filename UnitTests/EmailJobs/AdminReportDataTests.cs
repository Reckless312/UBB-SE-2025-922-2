// <copyright file="AdminReportDataTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.EmailJobs
{
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DrinkDb_Auth.ProxyRepository.AdminDashboard;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// tests AdminReprotData class.
    /// </summary>
    public class AdminReportDataTests
    {
        private DateTime reportDate = DateTime.Now;
        private List<User> adminUsers = new List<User>();
        private int activeUsersCount = 0;
        private int bannedUsersCount = 0;
        private int newReviewsCount = 0;
        private float averageRating = 4.5f;
        private List<Review> recentReviews = new List<Review>();
        private AdminReportData? reportData;

        /// <summary>
        /// tests that data initializes corectly.
        /// </summary>
        [Fact]
        public void AdminReportData_WhenCreated_InitializesCorrectly()
        {
            this.reportData = new AdminReportData(this.reportDate, this.adminUsers, this.activeUsersCount, this.bannedUsersCount, this.newReviewsCount, this.averageRating, this.recentReviews);
            Assert.Equal(this.reportDate, this.reportData.ReportDate);
            Assert.Same(this.adminUsers, this.reportData.AdminUsers);
            Assert.Equal(this.activeUsersCount, this.reportData.ActiveUsersCount);
            Assert.Equal(this.bannedUsersCount, this.reportData.BannedUsersCount);
            Assert.Equal(this.newReviewsCount, this.reportData.NewReviewsCount);
            Assert.Equal(this.averageRating, this.reportData.AverageRating);
            Assert.Same(this.recentReviews, this.reportData.RecentReviews);
        }

        /// <summary>
        /// tests that data initializes corectly.
        /// </summary>
        [Fact]
        public void AdminReportData_WithNonEmptyCollections_InitializesCorrectly()
        {
            List<User> adminUsers = new List<User> { new User { UserId = new Guid(), EmailAddress = "admin@example.com", Username = "Admin User", NumberOfDeletedReviews = 0, HasSubmittedAppeal = false, PasswordHash = String.Empty, TwoFASecret = String.Empty } };

            List<Review> recentReviews = new List<Review> { new Review(1, new Guid(), 4, "Great product", DateTime.Now) };

            this.reportData = new AdminReportData(this.reportDate, this.adminUsers, 10, 5, 3, 4.2, this.recentReviews);

            // Assert
            Assert.Equal(this.reportDate, this.reportData.ReportDate);
            Assert.Same(this.adminUsers, this.reportData.AdminUsers);
            Assert.Equal(10, this.reportData.ActiveUsersCount);
            Assert.Equal(5, this.reportData.BannedUsersCount);
            Assert.Equal(3, this.reportData.NewReviewsCount);
            Assert.Equal(4.2, this.reportData.AverageRating);
            Assert.Same(this.recentReviews, this.reportData.RecentReviews);
        }

        /// <summary>
        /// tests changing values.
        /// </summary>
        [Fact]
        public void AdminReportData_ModifyingProperties_ChangesValues()
        {
            this.reportData = new AdminReportData(this.reportDate, this.adminUsers, this.activeUsersCount, this.bannedUsersCount, this.newReviewsCount, this.averageRating, this.recentReviews);

            DateTime newDate = DateTime.Now.AddDays(1);
            List<User> newUsers = new List<User> { new User { UserId = new Guid(), EmailAddress = "email@adress.com", Username = "username", NumberOfDeletedReviews = 0, PasswordHash = String.Empty, TwoFASecret = String.Empty } };
            List<Review> newReviews = new List<Review> { new Review(3, new Guid(), 5, "New review", DateTime.Now) };

            this.reportData.ReportDate = newDate;
            this.reportData.AdminUsers = newUsers;
            this.reportData.ActiveUsersCount = 100;
            this.reportData.BannedUsersCount = 50;
            this.reportData.NewReviewsCount = 25;
            this.reportData.AverageRating = 3.7;
            this.reportData.RecentReviews = newReviews;

            Assert.Equal(newDate, this.reportData.ReportDate);
            Assert.Same(newUsers, this.reportData.AdminUsers);
            Assert.Equal(100, this.reportData.ActiveUsersCount);
            Assert.Equal(50, this.reportData.BannedUsersCount);
            Assert.Equal(25, this.reportData.NewReviewsCount);
            Assert.Equal(3.7, this.reportData.AverageRating);
            Assert.Same(newReviews, this.reportData.RecentReviews);
        }

        /// <summary>
        /// tests that data initializes corectly.
        /// </summary>
        [Fact]
        public void AdminReportData_WithExtremeDateValues_InitializesCorrectly()
        {
            // Arrange
            DateTime minDate = DateTime.MinValue;
            DateTime maxDate = DateTime.MaxValue;

            // Act & Assert
            AdminReportData data1 = new AdminReportData(minDate, this.adminUsers, this.activeUsersCount, this.bannedUsersCount, this.newReviewsCount, this.averageRating, this.recentReviews);
            Assert.Equal(minDate, data1.ReportDate);

            AdminReportData data2 = new AdminReportData(maxDate, this.adminUsers, this.activeUsersCount, this.bannedUsersCount, this.newReviewsCount, this.averageRating, this.recentReviews);
            Assert.Equal(maxDate, data2.ReportDate);
        }

        /// <summary>
        /// tests class initializes correctly.
        /// </summary>
        [Fact]
        public void AdminReportData_WithExtremeNumericValues_InitializesCorrectly()
        {
            int maxInt = int.MaxValue;
            int minInt = int.MinValue;
            double maxDouble = double.MaxValue;
            double minDouble = double.MinValue;
            AdminReportData data = new AdminReportData(this.reportDate, this.adminUsers, maxInt, minInt, maxInt, maxDouble, this.recentReviews);
            Assert.Equal(maxInt, data.ActiveUsersCount);
            Assert.Equal(minInt, data.BannedUsersCount);
            Assert.Equal(maxInt, data.NewReviewsCount);
            Assert.Equal(maxDouble, data.AverageRating);

            // Act
            data = new AdminReportData(this.reportDate, this.adminUsers, minInt, maxInt, minInt, minDouble, this.recentReviews);

            Assert.Equal(minInt, data.ActiveUsersCount);
            Assert.Equal(maxInt, data.BannedUsersCount);
            Assert.Equal(minInt, data.NewReviewsCount);
            Assert.Equal(minDouble, data.AverageRating);
        }
    }
}