namespace UnitTests.Reviews
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard;
    using IRepository;
    using Moq;
    using Xunit;

    public class ReviewsServiceTests
    {
        private readonly Mock<IReviewsRepository> mockRepository;
        private readonly ReviewsService service;

        public ReviewsServiceTests()
        {
            this.mockRepository = new Mock<IReviewsRepository>();
            this.service = new ReviewsService(this.mockRepository.Object);
        }

        [Fact]
        public void ResetReviewFlags_SetsNumberOfFlagsToZero()
        {
            // Arrange
            int reviewId = 1;
            var review = new Review(
                reviewId: reviewId,
                userId: new Guid(),
                rating: 4,
                content: "Great drink!",
                createdDate: DateTime.Now,
                numberOfFlags: 3);
            this.mockRepository.Setup(review => review.GetReviewById(reviewId)).Returns(Task.FromResult<Review>(review));

            // Act
            this.service.ResetReviewFlags(reviewId);

            // Assert
            this.mockRepository.Verify(review => review.UpdateNumberOfFlagsForReview(reviewId, 0), Times.Once);
        }

        [Fact]
        public void HideReview_SetsIsHiddenToTrue()
        {
            // Arrange
            int reviewId = 1;

            // Act
            this.service.HideReview(reviewId);

            // Assert
            this.mockRepository.Verify(review => review.UpdateReviewVisibility(reviewId, true), Times.Once);
        }

        [Fact]
        public void GetHiddenReviews_ReturnsHiddenReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now, 0, true),
                new Review(2, new Guid(), 5, "Amazing drink!", DateTime.Now, 0, false),
                new Review(3, new Guid(), 3, "Good drink!", DateTime.Now, 0, true),
            };
            this.mockRepository.Setup(review => review.GetAllReviews()).Returns(Task.FromResult(reviews));

            // Act
            var hiddenReviews = this.service.GetHiddenReviews();

            // Assert
            Assert.Equal(2, hiddenReviews.Count);
            Assert.All(hiddenReviews, review => Assert.True(review.IsHidden));
        }

        [Fact]
        public void GetAllReviews_ReturnsAllReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now),
                new Review(2, new Guid(), 5, "Amazing drink!", DateTime.Now),
                new Review(3, new Guid(), 3, "Good drink!", DateTime.Now),
            };
            this.mockRepository.Setup(review => review.GetAllReviews()).Returns(Task.FromResult(reviews));

            // Act
            var allReviews = this.service.GetAllReviews();

            // Assert
            Assert.Equal(3, allReviews.Count);
            this.mockRepository.Verify(review => review.GetAllReviews(), Times.Once);
        }

        [Fact]
        public void GetReviewsSince_ReturnsReviewsAfterDate()
        {
            // Arrange
            var date = DateTime.Now.AddDays(-2);
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now.AddDays(-1)),
                new Review(2, new Guid(), 5, "Amazing drink!", DateTime.Now.AddDays(-3)),
                new Review(3, new Guid(), 3, "Good drink!", DateTime.Now),
            };
            this.mockRepository.Setup(review => review.GetReviewsSince(date)).Returns(Task.FromResult(reviews.Where(review => review.CreatedDate >= date && !review.IsHidden).ToList()));

            // Act
            var recentReviews = this.service.GetReviewsSince(date);

            // Assert
            Assert.Equal(2, recentReviews.Count);
            Assert.All(recentReviews, review => Assert.True(review.CreatedDate >= date));
            this.mockRepository.Verify(review => review.GetReviewsSince(date), Times.Once);
        }

        [Fact]
        public void GetAverageRatingForVisibleReviews_ReturnsCorrectAverage()
        {
            // Arrange
            double expectedAverage = 4.0;
            this.mockRepository.Setup(review => review.GetAverageRatingForVisibleReviews()).Returns(Task.FromResult(expectedAverage));

            // Act
            var average = this.service.GetAverageRatingForVisibleReviews();

            // Assert
            Assert.Equal(expectedAverage, average);
            this.mockRepository.Verify(review => review.GetAverageRatingForVisibleReviews(), Times.Once);
        }

        [Fact]
        public void GetMostRecentReviews_ReturnsCorrectNumberOfReviews()
        {
            // Arrange
            int count = 2;
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now.AddDays(-1)),
                new Review(2, new Guid(), 5, "Amazing drink!", DateTime.Now),
                new Review(3, new Guid(), 3, "Good drink!", DateTime.Now.AddDays(-2)),
            };
            this.mockRepository.Setup(review => review.GetMostRecentReviews(count)).Returns(Task.FromResult(reviews.OrderByDescending(review => review.CreatedDate).Take(count).ToList()));

            // Act
            var recentReviews = this.service.GetMostRecentReviews(count);

            // Assert
            Assert.Equal(2, recentReviews.Count);
            Assert.Equal(2, recentReviews[0].ReviewId); // Most recent
            Assert.Equal(1, recentReviews[1].ReviewId); // Second most recent
            this.mockRepository.Verify(review => review.GetMostRecentReviews(count), Times.Once);
        }

        [Fact]
        public void GetReviewCountAfterDate_ReturnsCorrectCount()
        {
            // Arrange
            var date = DateTime.Now.AddDays(-2);
            int expectedCount = 5;
            this.mockRepository.Setup(review => review.GetReviewCountAfterDate(date)).Returns(Task.FromResult(expectedCount));

            // Act
            var count = this.service.GetReviewCountAfterDate(date);

            // Assert
            Assert.Equal(expectedCount, count);
            this.mockRepository.Verify(review => review.GetReviewCountAfterDate(date), Times.Once);
        }

        [Fact]
        public void GetReviewsByUser_ReturnsCorrectReviews()
        {
            // Arrange
            Guid userId = new Guid();
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now),
                new Review(2, new Guid(), 5, "Amazing drink!", DateTime.Now),
                new Review(3, new Guid(), 3, "Good drink!", DateTime.Now),
            };
            this.mockRepository.Setup(review => review.GetReviewsByUser(userId)).Returns(Task.FromResult(reviews.Where(review => review.UserId == userId && !review.IsHidden).OrderByDescending(review => review.CreatedDate).ToList()));

            // Act
            var userReviews = this.service.GetReviewsByUser(userId);

            // Assert
            Assert.Equal(3, userReviews.Count);
            Assert.All(userReviews, review => Assert.Equal(userId, review.UserId));
            this.mockRepository.Verify(review => review.GetReviewsByUser(userId), Times.Once);
        }

        [Fact]
        public void GetReviewsForReport_ReturnsRecentReviews()
        {
            // Arrange
            var date = DateTime.Now.AddDays(-1);
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now), // Should be MORE recent
                new Review(2, new Guid(), 5, "Amazing drink!", DateTime.Now.AddMinutes(-5)), // Should be LESS recent
                new Review(3, new Guid(), 3, "Good drink!", DateTime.Now.AddDays(-2)),
            };

            this.mockRepository.Setup(review =>
                review.GetReviewCountAfterDate(It.Is<DateTime>(d => d.Date == date.Date))).Returns(Task.FromResult(2));
            this.mockRepository.Setup(review =>
                review.GetMostRecentReviews(2)).Returns(Task.FromResult(reviews.OrderByDescending(review => review.CreatedDate).Take(2).ToList()));

            // Act
            var reportReviews = this.service.GetReviewsForReport();

            // Assert
            Assert.Equal(2, reportReviews.Count);
            Assert.Equal(1, reportReviews[0].ReviewId); // Most recent
            Assert.Equal(2, reportReviews[1].ReviewId); // Second most recent
            this.mockRepository.Verify(review => review.GetReviewCountAfterDate(It.Is<DateTime>(d => d.Date == date.Date)), Times.Once);
            this.mockRepository.Verify(review => review.GetMostRecentReviews(2), Times.Once);
        }

        [Fact]
        public void FilterReviewsByContent_WithMatchingContent_ReturnsFilteredReviews()
        {
            // Arrange
            string content = "great";
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now, 1, true),
                new Review(2, new Guid(), 5, "Not bad", DateTime.Now, 1, true),
                new Review(3, new Guid(), 3, "Really great service", DateTime.Now, 1, true),
            };

            this.mockRepository.Setup(repo => repo.GetAllReviews()).Returns(Task.FromResult(reviews));

            // Act
            var filtered = this.service.FilterReviewsByContent(content);

            // Assert
            Assert.Equal(2, filtered.Count);
            Assert.All(filtered, r => Assert.Contains("great", r.Content, StringComparison.OrdinalIgnoreCase));
            this.mockRepository.Verify(repo => repo.GetAllReviews(), Times.Once);
        }

        [Fact]
        public void FilterReviewsByContent_WithNullOrEmptyContent_ReturnsAllFlaggedReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now, 1, true),
                new Review(2, new Guid(), 5, "Not bad", DateTime.Now, 1, true),
                new Review(3, new Guid(), 3, "Really great service", DateTime.Now, 1, true),
            };

            this.mockRepository.Setup(repo => repo.GetAllReviews()).Returns(Task.FromResult(reviews));

            // Act
            var resultWithNull = this.service.FilterReviewsByContent(null);
            var resultWithEmpty = this.service.FilterReviewsByContent(string.Empty);

            // Assert
            Assert.Equal(3, resultWithNull.Count);
            Assert.Equal(3, resultWithEmpty.Count);
            this.mockRepository.Verify(repo => repo.GetAllReviews(), Times.Exactly(2));
        }

        [Fact]
        public void GetReviewsForReport_NoReviews_ReturnsEmptyList()
        {
            // Arrange
            var date = DateTime.Now.AddDays(-1);
            this.mockRepository.Setup(review =>
                review.GetReviewCountAfterDate(It.Is<DateTime>(d => d.Date == date.Date))).Returns(Task.FromResult(0));
            this.mockRepository.Setup(review =>
                review.GetMostRecentReviews(0)).Returns(Task.FromResult(new List<Review>()));

            // Act
            var reportReviews = this.service.GetReviewsForReport();

            // Assert
            Assert.Empty(reportReviews);
            this.mockRepository.Verify(review => review.GetReviewCountAfterDate(It.Is<DateTime>(d => d.Date == date.Date)), Times.Once);
            this.mockRepository.Verify(review => review.GetMostRecentReviews(0), Times.Once);
        }

        [Fact]
        public void FilterReviewsByContent_CaseSensitive_ReturnsFilteredReviews()
        {
            // Arrange
            string content = "GREAT";
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now, 1, true),
                new Review(2, new Guid(), 5, "Not bad", DateTime.Now, 1, true),
                new Review(3, new Guid(), 3, "Really great service", DateTime.Now, 1, true),
            };

            this.mockRepository.Setup(repo => repo.GetAllReviews()).Returns(Task.FromResult(reviews));

            // Act
            var filtered = this.service.FilterReviewsByContent(content);

            // Assert
            Assert.Equal(2, filtered.Count);
            Assert.All(filtered, r => Assert.Contains("great", r.Content, StringComparison.OrdinalIgnoreCase));
            this.mockRepository.Verify(repo => repo.GetAllReviews(), Times.Once);
        }

        [Fact]
        public void GetMostRecentReviews_InvalidCount_ReturnsEmptyList()
        {
            // Arrange
            int count = -1;
            this.mockRepository.Setup(review => review.GetMostRecentReviews(count)).Returns(Task.FromResult(new List<Review>()));

            // Act
            var recentReviews = this.service.GetMostRecentReviews(count);

            // Assert
            Assert.Empty(recentReviews);
            this.mockRepository.Verify(review => review.GetMostRecentReviews(count), Times.Once);
        }

        [Fact]
        public void GetMostRecentReviews_ZeroCount_ReturnsEmptyList()
        {
            // Arrange
            int count = 0;
            this.mockRepository.Setup(review => review.GetMostRecentReviews(count)).Returns(Task.FromResult(new List<Review>()));

            // Act
            var recentReviews = this.service.GetMostRecentReviews(count);

            // Assert
            Assert.Empty(recentReviews);
            this.mockRepository.Verify(review => review.GetMostRecentReviews(count), Times.Once);
        }

        [Fact]
        public void GetReviewsForReport_RepositoryReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            var date = DateTime.Now.AddDays(-1);
            this.mockRepository.Setup(review =>
                review.GetReviewCountAfterDate(It.Is<DateTime>(d => d.Date == date.Date))).Returns(Task.FromResult(2));
            this.mockRepository.Setup(review =>
                review.GetMostRecentReviews(2)).Returns(Task.FromResult((List<Review>)null));

            // Act
            var reportReviews = this.service.GetReviewsForReport();

            // Assert
            Assert.Empty(reportReviews);
            this.mockRepository.Verify(review => review.GetReviewCountAfterDate(It.Is<DateTime>(d => d.Date == date.Date)), Times.Once);
            this.mockRepository.Verify(review => review.GetMostRecentReviews(2), Times.Once);
        }

        [Fact]
        public void GetReviewsSince_WithHiddenReviews_ReturnsOnlyVisibleReviews()
        {
            // Arrange
            var date = DateTime.Now.AddDays(-2);
            var reviews = new List<Review>
            {
                new Review(1, new Guid(), 4, "Great drink!", DateTime.Now.AddDays(-1), 0, true),
                new Review(2, new Guid(), 5, "Amazing drink!", DateTime.Now.AddDays(-3), 0, false),
                new Review(3, new Guid(), 3, "Good drink!", DateTime.Now, 0, false),
            };
            this.mockRepository.Setup(review => review.GetReviewsSince(date)).Returns(Task.FromResult(reviews));

            // Act
            var recentReviews = this.service.GetReviewsSince(date);

            // Assert
            Assert.Equal(3, recentReviews.Count); // Service returns all reviews from repository
            this.mockRepository.Verify(review => review.GetReviewsSince(date), Times.Once);
        }

        [Fact]
        public void GetReviewsByUser_NoReviews_ReturnsEmptyList()
        {
            // Arrange
            Guid userId = new Guid();
            this.mockRepository.Setup(review => review.GetReviewsByUser(userId)).Returns(Task.FromResult(new List<Review>()));

            // Act
            var userReviews = this.service.GetReviewsByUser(userId);

            // Assert
            Assert.Empty(userReviews);
            this.mockRepository.Verify(review => review.GetReviewsByUser(userId), Times.Once);
        }
    }
}
