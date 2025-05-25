namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    public interface IReviewService
    {
        Task<int> AddReview(Review review);

        Task RemoveReviewById(int reviewId);

        Task<Review?> GetReviewById(int reviewId);

        Task UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags);

        Task UpdateReviewVisibility(int reviewId, bool isHidden);

        Task HideReview(int reviewID);

        Task<List<Review>> GetFlaggedReviews(int minFlags = 1);

        Task<List<Review>> GetHiddenReviews();

        Task<List<Review>> GetAllReviews();

        Task<List<Review>> GetReviewsSince(DateTime date);

        Task<double> GetAverageRatingForVisibleReviews();

        Task<List<Review>> GetMostRecentReviews(int count);

        Task<List<Review>> GetReviewsForReport();

        Task<int> GetReviewCountAfterDate(DateTime date);

        Task<List<Review>> GetReviewsByUser(Guid userId);

        Task ResetReviewFlags(int reviewId);

        Task<List<Review>> FilterReviewsByContent(string content);
    }
}
