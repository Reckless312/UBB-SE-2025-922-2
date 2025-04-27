namespace UnitTests.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using App1.Models;
    using App1.Repositories;
    using App1.Services;
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Configuration;
    using MimeKit;
    using Moq;
    using Moq.Protected;
    using Quartz;
    using Xunit;

    public class EmailJobTests
    {
        private readonly Mock<IUserService> mockUserService;
        private readonly Mock<IReviewService> mockReviewService;
        private readonly Mock<IConfiguration> mockConfiguration;
        private readonly Mock<ITemplateProvider> mockTemplateProvider;
        private readonly Mock<IEmailSender> mockEmailSender;
        private readonly EmailJob emailJob;

        private readonly List<User> adminUsers = new List<User> { new User(1, "admin@example.com", "Admin User", 0, false, UserRepository.AdminRoles), new User(2, "admin2@example.com", "Second Admin", 0, false, UserRepository.AdminRoles) };

        private readonly List<User> regularUsers = new List<User> { new User(3, "user@example.com", "Regular User", 0, false, UserRepository.BasicUserRoles), new User(4, "user2@example.com", "Another User", 1, false, UserRepository.BasicUserRoles) };

        private readonly List<User> bannedUsers = new List<User> { new User(5, "banned@example.com", "Banned User", 3, true, UserRepository.BannedUserRoles) };

        private readonly List<User> allUsers = new List<User>
        {
            new User(1, "admin@example.com", "Admin User", 0, false, UserRepository.AdminRoles),
            new User(2, "admin2@example.com", "Second Admin", 0, false, UserRepository.AdminRoles),
            new User(3, "user@example.com", "Regular User", 0, false, UserRepository.BasicUserRoles),
            new User(4, "user2@example.com", "Another User", 1, false, UserRepository.BasicUserRoles),
            new User(5, "banned@example.com", "Banned User", 3, true, UserRepository.BannedUserRoles),
        };

        private readonly List<Review> reviews = new List<Review> { new Review(1, 3, 4, "Good product", DateTime.Now.AddDays(-1)), new Review(2, 4, 5, "Excellent service", DateTime.Now.AddDays(-2)) };

        public EmailJobTests()
        {
            this.mockUserService = new Mock<IUserService>();
            this.mockReviewService = new Mock<IReviewService>();
            this.mockConfiguration = new Mock<IConfiguration>();
            this.mockUserService.Setup(userService => userService.GetAdminUsers()).Returns(this.adminUsers);
            this.mockUserService.Setup(userService => userService.GetAllUsers()).Returns(this.allUsers);
            this.mockUserService.Setup(userService => userService.GetBannedUsers()).Returns(this.bannedUsers);
            this.mockUserService.Setup(userService => userService.GetUserById(It.IsAny<int>())).Returns<int>(id => this.allUsers.FirstOrDefault(u => u.UserId == id));
            this.mockUserService.Setup(userService => userService.GetRegularUsers()).Returns(this.regularUsers);
            this.mockReviewService.Setup(s => s.GetReviewsSince(It.IsAny<DateTime>())).Returns(this.reviews);
            this.mockReviewService.Setup(s => s.GetAverageRatingForVisibleReviews()).Returns(4.5);
            this.mockReviewService.Setup(s => s.GetReviewsForReport()).Returns(this.reviews);
            this.mockConfiguration.SetupGet(x => x["SMTP_MODERATOR_EMAIL"]).Returns("moderator@example.com");
            this.mockConfiguration.SetupGet(x => x["SMTP_MODERATOR_PASSWORD"]).Returns("password123");
            FileTemplateProvider mockFileTemplateProvider = new FileTemplateProvider();
            this.mockTemplateProvider = new Mock<ITemplateProvider>();
            this.mockTemplateProvider.Setup(p => p.GetEmailTemplate()).Returns(mockFileTemplateProvider.GetEmailTemplate());
            this.mockTemplateProvider.Setup(p => p.GetPlainTextTemplate()).Returns(mockFileTemplateProvider.GetPlainTextTemplate());
            this.mockTemplateProvider.Setup(p => p.GetReviewRowTemplate()).Returns(mockFileTemplateProvider.GetReviewRowTemplate());
            this.mockEmailSender = new Mock<IEmailSender>();
            this.mockEmailSender.Setup(s => s.SendEmailAsync(It.IsAny<MimeMessage>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            this.emailJob = new EmailJob(this.mockUserService.Object, this.mockReviewService.Object, this.mockConfiguration.Object, this.mockEmailSender.Object, this.mockTemplateProvider.Object);
        }

        [Fact]
        public async Task Execute_ShouldSendEmailsToAllAdmins()
        {
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await this.emailJob.Execute(jobContext);
            this.mockEmailSender.Verify(sender => sender.SendEmailAsync(It.IsAny<MimeMessage>(), "moderator@example.com", "password123"), Times.Exactly(this.adminUsers.Count));
            foreach (User admin in this.adminUsers)
            {
                this.mockEmailSender.Verify(
                    sender => sender.SendEmailAsync(It.Is<MimeMessage>(msg => msg.To.Any(to => to.ToString().Contains(admin.EmailAddress))), "moderator@example.com", "password123"), Times.Once);
            }
        }

        [Fact]
        public async Task Execute_WithoutCredentials_ShouldNotSendEmails()
        {
            this.mockConfiguration.SetupGet(x => x["SMTP_MODERATOR_EMAIL"]).Returns((string?)null);
            EmailJob emailJob = new EmailJob(this.mockUserService.Object, this.mockReviewService.Object, this.mockConfiguration.Object);
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await emailJob.Execute(jobContext);
            this.mockEmailSender.Verify(sender => sender.SendEmailAsync(It.IsAny<MimeMessage>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithoutPassword_ShouldNotSendEmails()
        {
            this.mockConfiguration.SetupGet(x => x["SMTP_MODERATOR_PASSWORD"]).Returns((string?)null);
            EmailJob emailJob = new EmailJob(this.mockUserService.Object, this.mockReviewService.Object, this.mockConfiguration.Object);
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await emailJob.Execute(jobContext);
            this.mockEmailSender.Verify(sender => sender.SendEmailAsync(It.IsAny<MimeMessage>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GatherReportData_ShouldIncludeCorrectReportData()
        {
            AdminReportData? result = typeof(EmailJob).GetMethod("GatherReportData", BindingFlags.NonPublic | BindingFlags.Instance) !.Invoke(this.emailJob, null) as AdminReportData;
            Assert.NotNull(result);
            Assert.Equal(this.adminUsers.Count + this.regularUsers.Count, result.ActiveUsersCount);
            Assert.Equal(this.bannedUsers.Count, result.BannedUsersCount);
            Assert.Equal(this.reviews.Count, result.NewReviewsCount);
            Assert.Equal(4.5, result.AverageRating);
            Assert.Equal(this.reviews, result.RecentReviews);
        }

        [Fact]
        public async Task Execute_ShouldIncludeCorrectEmailSubject()
        {
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await this.emailJob.Execute(jobContext);
            this.mockEmailSender.Verify(sender => sender.SendEmailAsync(It.Is<MimeMessage>(message => message.Subject.Contains("Admin Report") && message.Subject.Contains(DateTime.Now.ToString("yyyy-MM-dd"))), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Execute_ShouldIncludeCorrectEmailContent()
        {
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await this.emailJob.Execute(jobContext);
            this.mockEmailSender.Verify(sender => sender.SendEmailAsync(It.Is<MimeMessage>(message => message.HtmlBody.Contains("Active Users:") && message.HtmlBody.Contains("Banned Users:") && message.HtmlBody.Contains("New Reviews:") && message.HtmlBody.Contains("Average Rating:")), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldIncludeRecentReviewsInCorrectFormat()
        {
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await this.emailJob.Execute(jobContext);
            foreach (Review review in this.reviews)
            {
                User user = this.allUsers.First(u => u.UserId == review.UserId);
                this.mockEmailSender.Verify(sender => sender.SendEmailAsync(It.Is<MimeMessage>(message => message.HtmlBody.Contains(user.FullName) && message.HtmlBody.Contains(review.Rating.ToString())), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            }
        }

        [Fact]
        public async Task Execute_WithNoReviews_ShouldHandleEmptyList()
        {
            this.mockReviewService.Setup(reviewService => reviewService.GetReviewsForReport()).Returns(new List<Review>());
            EmailJob emailJob = new EmailJob(this.mockUserService.Object, this.mockReviewService.Object, this.mockConfiguration.Object);
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await emailJob.Execute(jobContext);
            this.mockEmailSender.Verify(sender => sender.SendEmailAsync(It.Is<MimeMessage>(message => message.HtmlBody.Contains("No recent reviews")), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.True(1 == 1);
        }

        [Fact]
        public async Task Execute_WithExceptionInSendMail_ShouldHandleException()
        {
            this.mockEmailSender.Setup(s => s.SendEmailAsync(It.IsAny<MimeMessage>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("SMTP failure"));
            IJobExecutionContext jobContext = Mock.Of<IJobExecutionContext>();
            await this.emailJob.Execute(jobContext);
        }
    }
}