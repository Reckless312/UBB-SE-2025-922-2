namespace UnitTests.Reviews
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using App1.Models;
    using App1.Repositories;
    using Xunit;

    public class ReviewsRepositoryTests
    {
        [Fact]
        public void LoadsReviews_InvalidData_ThrowsException()
        {
            ReviewsRepository repository = new ReviewsRepository();
            Assert.Throws<ArgumentNullException>(() => repository.LoadReviews(null));
        }

        [Fact]
        public void GetAllReviews_ReturnsAllReviews()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> reviews = repository.GetAllReviews();
            Assert.NotNull(reviews);
            Assert.True(reviews.Count > 0);
        }

        [Fact]
        public void GetReviewsSince_ReturnsReviewsAfterDate()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            DateTime date = DateTime.Now.AddDays(-2);
            List<Review> reviews = repository.GetReviewsSince(date);
            Assert.NotNull(reviews);
            Assert.All(reviews, review => Assert.True(review.CreatedDate >= date));
            Assert.All(reviews, review => Assert.False(review.IsHidden));
        }

        [Fact]
        public void GetAverageRatingForVisibleReviews_ReturnsCorrectAverage()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> allReviews = repository.GetAllReviews();
            List<Review> visibleReviews = allReviews.Where(review => !review.IsHidden).ToList();
            double expectedAverage = visibleReviews.Any() ? Math.Round(visibleReviews.Average(review => review.Rating), 1) : 0.0;
            double average = repository.GetAverageRatingForVisibleReviews();
            Assert.Equal(expectedAverage, average);
        }

        [Fact]
        public void GetAverageRatingForVisibleReviews_NoVisibleReviews_ReturnsZero()
        {
            ReviewsRepository repository = new ReviewsRepository();
            double expectedAverage = 0.0;
            double average = repository.GetAverageRatingForVisibleReviews();
            Assert.Equal(expectedAverage, average);
        }

        [Fact]
        public void GetMostRecentReviews_ReturnsCorrectNumberOfReviews()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            int count = 3;
            List<Review> reviews = repository.GetMostRecentReviews(count);
            Assert.NotNull(reviews);
            Assert.True(reviews.Count <= count);
            Assert.All(reviews, review => Assert.False(review.IsHidden));
            for (int i = 0; i < reviews.Count - 1; i++)
            {
                Assert.True(reviews[i].CreatedDate >= reviews[i + 1].CreatedDate);
            }
        }

        [Fact]
        public void GetReviewCountAfterDate_ReturnsCorrectCount()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            DateTime date = DateTime.Now.AddDays(-2);
            List<Review> allReviews = repository.GetAllReviews();
            int expectedCount = allReviews.Count(review => review.CreatedDate >= date && !review.IsHidden);
            int count = repository.GetReviewCountAfterDate(date);
            Assert.Equal(expectedCount, count);
        }

        [Fact]
        public void GetFlaggedReviews_ReturnsReviewsWithMinFlags()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            int minFlags = 1;
            List<Review> allReviews = repository.GetAllReviews();
            List<Review> expectedReviews = allReviews.Where(review => review.NumberOfFlags >= minFlags && !review.IsHidden).ToList();
            List<Review> reviews = repository.GetFlaggedReviews(minFlags);
            Assert.NotNull(reviews);
            Assert.Equal(expectedReviews.Count, reviews.Count);
            Assert.All(reviews, review => Assert.True(review.NumberOfFlags >= minFlags));
            Assert.All(reviews, review => Assert.False(review.IsHidden));
        }

        [Fact]
        public void GetReviewsByUser_ReturnsCorrectReviews()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            int userId = 1;
            List<Review> allReviews = repository.GetAllReviews();
            List<Review> expectedReviews = allReviews.Where(review => review.UserId == userId && !review.IsHidden).ToList();
            List<Review> reviews = repository.GetReviewsByUser(userId);
            Assert.NotNull(reviews);
            Assert.Equal(expectedReviews.Count, reviews.Count);
            Assert.All(reviews, review => Assert.Equal(userId, review.UserId));
            Assert.All(reviews, review => Assert.False(review.IsHidden));
            for (int i = 0; i < reviews.Count - 1; i++)
            {
                Assert.True(reviews[i].CreatedDate >= reviews[i + 1].CreatedDate);
            }
        }

        [Fact]
        public void GetReviewById_ReturnsCorrectReview()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> allReviews = repository.GetAllReviews();
            int reviewId = allReviews.First().ReviewId;
            Review review = repository.GetReviewById(reviewId);
            Assert.NotNull(review);
            Assert.Equal(reviewId, review.ReviewId);
        }

        [Fact]
        public void GetReviewById_ReturnsNullForNonExistentId()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            int nonExistentId = 9999;
            Review review = repository.GetReviewById(nonExistentId);
            Assert.Null(review);
        }

        [Fact]
        public void UpdateReviewVisibility_UpdatesIsHidden()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> allReviews = repository.GetAllReviews();
            int reviewId = allReviews.First().ReviewId;
            bool isHidden = true;
            repository.UpdateReviewVisibility(reviewId, isHidden);

            Review updatedReview = repository.GetReviewById(reviewId);
            Assert.NotNull(updatedReview);
            Assert.Equal(isHidden, updatedReview.IsHidden);
        }

        [Fact]
        public void UpdateNumberOfFlagsForReview_UpdatesNumberOfFlags()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> allReviews = repository.GetAllReviews();
            int reviewId = allReviews.First().ReviewId;
            int numberOfFlags = 5;
            repository.UpdateNumberOfFlagsForReview(reviewId, numberOfFlags);
            Review updatedReview = repository.GetReviewById(reviewId);
            Assert.NotNull(updatedReview);
            Assert.Equal(numberOfFlags, updatedReview.NumberOfFlags);
        }

        [Fact]
        public void AddReview_ReturnsNewId()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> allReviews = repository.GetAllReviews();
            int maxId = allReviews.Max(review => review.ReviewId);
            Review newReview = new Review(reviewId: 0, userId: 5, rating: 4, content: "New review", createdDate: DateTime.Now);
            int newId = repository.AddReview(newReview);
            Assert.True(newId > maxId);
            Review addedReview = repository.GetReviewById(newId);
            Assert.NotNull(addedReview);
            Assert.Equal(newReview.UserId, addedReview.UserId);
            Assert.Equal(newReview.Rating, addedReview.Rating);
            Assert.Equal(newReview.Content, addedReview.Content);
        }

        [Fact]
        public void RemoveReviewById_RemovesReview()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> allReviews = repository.GetAllReviews();
            int reviewId = allReviews.First().ReviewId;
            bool result = repository.RemoveReviewById(reviewId);
            Assert.True(result);
            Review removedReview = repository.GetReviewById(reviewId);
            Assert.Null(removedReview);
        }

        [Fact]
        public void RemoveReviewById_ReturnsFalseForNonExistentId()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            int nonExistentId = 9999;
            bool result = repository.RemoveReviewById(nonExistentId);
            Assert.False(result);
        }

        [Fact]
        public void GetHiddenReviews_ReturnsHiddenReviews()
        {
            ReviewsRepository repository = this.CreateRepositoryWithTestData();
            List<Review> allReviews = repository.GetAllReviews();
            int reviewId = allReviews.First().ReviewId;
            repository.UpdateReviewVisibility(reviewId, true);
            List<Review> hiddenReviews = repository.GetHiddenReviews();
            Assert.NotNull(hiddenReviews);
            Assert.Contains(hiddenReviews, review => review.ReviewId == reviewId);
            Assert.All(hiddenReviews, review => Assert.True(review.IsHidden));
        }

        private static IEnumerable<Review> CreateTestReviews()
        {
            return new List<Review>
            {
                new Review(
                    reviewId: 0,
                    userId: 1,
                    rating: 5,
                    content: "Terrible mix, a complete mess dick ass taste",
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
                    userId: 2,
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
                    userId: 2,
                    rating: 5,
                    content: "Amazing",
                    createdDate: DateTime.Now.AddDays(-2),
                    isHidden: false),
                new Review(
                    reviewId: 0,
                    userId: 2,
                    rating: 5,
                    content: "My favorite!",
                    createdDate: DateTime.Now.AddDays(-2),
                    isHidden: false),
            };
        }

        private ReviewsRepository CreateRepositoryWithTestData()
        {
            ReviewsRepository repository = new ReviewsRepository();
            repository.LoadReviews(CreateTestReviews());
            return repository;
        }
    }
}
