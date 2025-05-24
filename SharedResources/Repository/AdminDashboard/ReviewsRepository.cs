namespace Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using ServerAPI.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Data.SqlClient;
    public class ReviewsRepository : IReviewsRepository
    {
        private readonly DatabaseContext dataContext;

        public ReviewsRepository(DatabaseContext context)
        {
            this.dataContext = context;
        }

        public async Task LoadReviews(IEnumerable<Review> reviewsToLoad)
        {
            await this.dataContext.Reviews.AddRangeAsync(reviewsToLoad);
            await this.dataContext.SaveChangesAsync();
        }

        public async Task<List<Review>> GetAllReviews()
        {
            return await this.dataContext.Reviews.ToListAsync();
        }

        public async Task<List<Review>> GetReviewsSince(DateTime date)
        {
            return await this.dataContext.Reviews
                .Where(review => review.CreatedDate >= date && !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            List<Review> visibleReviews = await this.dataContext.Reviews
                .Where(review => !review.IsHidden)
                .ToListAsync();

            if (!visibleReviews.Any())
            {
                return 0.0;
            }
            return Math.Round(visibleReviews.Average(r => r.Rating), 1);
        }

        public async Task<List<Review>> GetMostRecentReviews(int count)
        {
            return await this.dataContext.Reviews
                .Where(review => !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetReviewCountAfterDate(DateTime date)
        {
            return await this.dataContext.Reviews
                .CountAsync(review => review.CreatedDate >= date && !review.IsHidden);
        }

        public async Task<List<Review>> GetFlaggedReviews(int minFlags)
        {
            return await this.dataContext.Reviews
                .Where(review => review.NumberOfFlags >= minFlags && !review.IsHidden)
                .ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUser(Guid userId)
        {
            return await this.dataContext.Reviews
                .Where(review => review.UserId == userId && !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewById(int reviewId)
        {
            return await this.dataContext.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        // I changed this from task to void cause no await was used in the original code.
        public void UpdateReviewVisibility(int reviewId, bool isHidden)
        {
            Review? review = this.dataContext.Reviews.Find(reviewId);

            if (review == null)
            {
                return;
            }

            review.IsHidden = isHidden;
            this.dataContext.Reviews.Update(review);
            this.dataContext.SaveChanges();
        }

        public void UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags)
        {
            Review? review = this.dataContext.Reviews.Find(reviewId);

            if (review == null)
            {
                return;
            }

            review.NumberOfFlags = numberOfFlags;
            this.dataContext.Reviews.Update(review);
            this.dataContext.SaveChanges();
        }

        public async Task<int> AddReview(Review review)
        {
            await this.dataContext.Reviews.AddAsync(review);
            await this.dataContext.SaveChangesAsync();
            return review.ReviewId;
        }

        public async Task RemoveReviewById(int reviewId)
        {
            Review? review = await this.GetReviewById(reviewId);

            if (review == null)
            {
                return;
            }

            this.dataContext.Reviews.Remove(review);
            await this.dataContext.SaveChangesAsync();
        }

        public async Task<List<Review>> GetHiddenReviews()
        {
            return await this.dataContext.Reviews
                .Where(review => review.IsHidden)
                .ToListAsync();
        }
    }
}