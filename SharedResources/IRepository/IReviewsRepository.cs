namespace IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    /// <summary>
    /// Interface for the Review repository that defines operations for managing reviews.
    /// </summary>
    public interface IReviewsRepository
    {
        /// <summary>
        /// Gets all reviews.
        /// </summary>
        /// <returns>A list of all reviews.</returns>
        Task<List<Review>> GetAllReviews();

        /// <summary>
        /// Gets reviews created since a specific date.
        /// </summary>
        /// <param name="date">The date to filter reviews from.</param>
        /// <returns>A list of reviews created since the specified date.</returns>
        Task<List<Review>> GetReviewsSince(DateTime date);

        /// <summary>
        /// Gets the average rating of all visible reviews.
        /// </summary>
        /// <returns>The average rating.</returns>
        Task<double> GetAverageRatingForVisibleReviews();

        /// <summary>
        /// Gets the most recent reviews.
        /// </summary>
        /// <param name="count">The number of reviews to retrieve.</param>
        /// <returns>A list of the most recent reviews.</returns>
        Task<List<Review>> GetMostRecentReviews(int count);

        /// <summary>
        /// Gets the count of reviews created after a specific date.
        /// </summary>
        /// <param name="date">The date to count reviews from.</param>
        /// <returns>The number of reviews created after the specified date.</returns>
        Task<int> GetReviewCountAfterDate(DateTime date);

        /// <summary>
        /// Gets reviews that have been flagged a minimum number of times.
        /// </summary>
        /// <param name="minFlags">The minimum number of flags required.</param>
        /// <returns>A list of flagged reviews.</returns>
        Task<List<Review>> GetFlaggedReviews(int minFlags);

        /// <summary>
        /// Gets reviews by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of reviews by the specified user.</returns>
        Task<List<Review>> GetReviewsByUser(Guid userId);

        /// <summary>
        /// Gets a review by its ID.
        /// </summary>
        /// <param name="reviewID">The ID of the review.</param>
        /// <returns>The review with the specified ID, or null if not found.</returns>
        Task<Review> GetReviewById(int reviewID);

        /// <summary>
        /// Updates the hidden status of a review.
        /// </summary>
        /// <param name="reviewID">The ID of the review.</param>
        /// <param name="isHidden">Whether the review should be hidden.</param>
        Task UpdateReviewVisibility(int reviewID, bool isHidden);

        /// <summary>
        /// Updates the number of flags for a review.
        /// </summary>
        /// <param name="reviewID">The ID of the review.</param>
        /// <param name="numberOfFlags">The new number of flags.</param>
        Task UpdateNumberOfFlagsForReview(int reviewID, int numberOfFlags);

        /// <summary>
        /// Adds a new review.
        /// </summary>
        /// <param name="review">The review to add.</param>
        /// <returns>The ID of the newly added review.</returns>
        Task<int> AddReview(Review review);

        /// <summary>
        /// Removes a review by its ID.
        /// </summary>
        /// <param name="reviewID">The ID of the review to remove.</param>
        /// <returns>True if the review was removed, false otherwise.</returns>
        Task<bool> RemoveReviewById(int reviewID);

        /// <summary>
        /// Gets all hidden reviews.
        /// </summary>
        /// <returns>A list of hidden reviews.</returns>
        Task<List<Review>> GetHiddenReviews();
    }
}
