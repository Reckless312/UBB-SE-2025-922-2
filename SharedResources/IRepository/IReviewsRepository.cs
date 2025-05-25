namespace IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    public interface IReviewsRepository
    {
        Task<List<Review>> GetAllReviews();

        Task<List<Review>> GetReviewsSince(DateTime date);

        Task<double> GetAverageRatingForVisibleReviews();

        Task<List<Review>> GetMostRecentReviews(int count);

        Task<int> GetReviewCountAfterDate(DateTime date);

        Task<List<Review>> GetFlaggedReviews(int minFlags);

        Task<List<Review>> GetReviewsByUser(Guid userId);

        Task<Review?> GetReviewById(int reviewID);

        Task UpdateReviewVisibility(int reviewID, bool isHidden);

        Task UpdateNumberOfFlagsForReview(int reviewID, int numberOfFlags);

        Task<int> AddReview(Review review);

        Task RemoveReviewById(int reviewID);

        Task<List<Review>> GetHiddenReviews();
    }
}
