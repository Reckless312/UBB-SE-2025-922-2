namespace DrinkDb_Auth.Service.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using IRepository;

    public class ReviewsService : IReviewService
    {
        private IReviewsRepository reviewsRepository;
        private const int REVIEW_ID_FAILURE = -1;

        public ReviewsService(IReviewsRepository reviewsRepository)
        {
            this.reviewsRepository = reviewsRepository;
        }

        public async Task<int> AddReview(Review review)
        {
            try
            {
                return await this.reviewsRepository.AddReview(review);
            }
            catch
            {
                return ReviewsService.REVIEW_ID_FAILURE;
            }
        }

        public async Task RemoveReviewById(int reviewId)
        {
            try
            {
                await this.reviewsRepository.RemoveReviewById(reviewId);
            }
            catch
            {
            }
        }

        public async Task<Review?> GetReviewById(int reviewId)
        {
            try
            {
                return await this.reviewsRepository.GetReviewById(reviewId);
            }
            catch
            {
                return null;
            }
        }

        public void UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags)
        {
            try
            {
                this.reviewsRepository.UpdateNumberOfFlagsForReview(reviewId, numberOfFlags);
            }
            catch
            {
            }
        }

        public void UpdateReviewVisibility(int reviewId, bool isHidden)
        {
            try
            {
                this.reviewsRepository.UpdateReviewVisibility(reviewId, isHidden);
            }
            catch
            {
            }
        }

        public void ResetReviewFlags(int reviewId)
        {
            try
            {
                this.reviewsRepository.UpdateNumberOfFlagsForReview(reviewId, 0);
            }
            catch
            {
            }
        }

        public void HideReview(int reviewId)
        {
            try
            {
                this.reviewsRepository.UpdateReviewVisibility(reviewId, true);
            }
            catch
            {
            }
        }

        public async Task<List<Review>> GetFlaggedReviews(int minFlags = 1)
        {
            try
            {
                return await this.reviewsRepository.GetFlaggedReviews(minFlags);
            }
            catch
            {
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetHiddenReviews()
        {
            try
            {
                List<Review> reviews = await this.reviewsRepository.GetAllReviews();
                return reviews.Where(review => review.IsHidden == true).ToList();
            }
            catch
            {
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetAllReviews()
        {
            try
            {
                return await this.reviewsRepository.GetAllReviews();
            }
            catch
            {
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetReviewsSince(DateTime date)
        {
            try
            {
                return await this.reviewsRepository.GetReviewsSince(date);
            }
            catch
            {
                return new List<Review>();
            }
        }

        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            try
            {
                return await this.reviewsRepository.GetAverageRatingForVisibleReviews();
            }
            catch
            {
                return 0.0;
            }
        }

        public async Task<List<Review>> GetMostRecentReviews(int count)
        {
            try
            {
                return await this.reviewsRepository.GetMostRecentReviews(count);
            }
            catch
            {
                return new List<Review>();
            }
        }

        public async Task<int> GetReviewCountAfterDate(DateTime date)
        {
            try
            {
                return await this.reviewsRepository.GetReviewCountAfterDate(date);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<Review>> GetReviewsByUser(Guid userId)
        {
            try
            {
                return await this.reviewsRepository.GetReviewsByUser(userId);
            }
            catch
            {
                return new List<Review>();
            }
        }

        public async Task<List<Review>> GetReviewsForReport()
        {
            try
            {
                DateTime date = DateTime.Now.AddDays(-1);
                int count = await this.reviewsRepository.GetReviewCountAfterDate(date);

                List<Review> reviews = await this.reviewsRepository.GetMostRecentReviews(count);
                return reviews ?? [];
            }
            catch
            {
                return new List<Review>();
            }
        }

        public async Task<List<Review>> FilterReviewsByContent(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                {
                    return await this.GetFlaggedReviews();
                }

                content = content.ToLower();
                List<Review> reviews = await this.GetFlaggedReviews();
                return reviews.Where(review => review.Content.ToLower().Contains(content)).ToList();
            }
            catch
            {
                return new List<Review>();
            }
        }
    }
}