namespace Repository.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Model.AdminDashboard;

    /// <summary>
    /// Interface for the Review repository that defines operations for managing reviews.
    /// </summary>
    public interface IReviewsRepository
    {
        /// <summary>
        /// Gets all reviews.
        /// </summary>
        /// <returns>A list of all reviews.</returns>
        List<Review> GetAllReviews();

        /// <summary>
        /// Gets reviews created since a specific date.
        /// </summary>
        /// <param name="date">The date to filter reviews from.</param>
        /// <returns>A list of reviews created since the specified date.</returns>
        List<Review> GetReviewsSince(DateTime date);

        /// <summary>
        /// Gets the average rating of all visible reviews.
        /// </summary>
        /// <returns>The average rating.</returns>
        double GetAverageRatingForVisibleReviews();

        /// <summary>
        /// Gets the most recent reviews.
        /// </summary>
        /// <param name="count">The number of reviews to retrieve.</param>
        /// <returns>A list of the most recent reviews.</returns>
        List<Review> GetMostRecentReviews(int count);

        /// <summary>
        /// Gets the count of reviews created after a specific date.
        /// </summary>
        /// <param name="date">The date to count reviews from.</param>
        /// <returns>The number of reviews created after the specified date.</returns>
        int GetReviewCountAfterDate(DateTime date);

        /// <summary>
        /// Gets reviews that have been flagged a minimum number of times.
        /// </summary>
        /// <param name="minFlags">The minimum number of flags required.</param>
        /// <returns>A list of flagged reviews.</returns>
        List<Review> GetFlaggedReviews(int minFlags);

        /// <summary>
        /// Gets reviews by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of reviews by the specified user.</returns>
        List<Review> GetReviewsByUser(Guid userId);

        /// <summary>
        /// Gets a review by its ID.
        /// </summary>
        /// <param name="reviewID">The ID of the review.</param>
        /// <returns>The review with the specified ID, or null if not found.</returns>
        Review GetReviewById(int reviewID);

        /// <summary>
        /// Updates the hidden status of a review.
        /// </summary>
        /// <param name="reviewID">The ID of the review.</param>
        /// <param name="isHidden">Whether the review should be hidden.</param>
        void UpdateReviewVisibility(int reviewID, bool isHidden);

        /// <summary>
        /// Updates the number of flags for a review.
        /// </summary>
        /// <param name="reviewID">The ID of the review.</param>
        /// <param name="numberOfFlags">The new number of flags.</param>
        void UpdateNumberOfFlagsForReview(int reviewID, int numberOfFlags);

        /// <summary>
        /// Adds a new review.
        /// </summary>
        /// <param name="review">The review to add.</param>
        /// <returns>The ID of the newly added review.</returns>
        int AddReview(Review review);

        /// <summary>
        /// Removes a review by its ID.
        /// </summary>
        /// <param name="reviewID">The ID of the review to remove.</param>
        /// <returns>True if the review was removed, false otherwise.</returns>
        bool RemoveReviewById(int reviewID);

        /// <summary>
        /// Gets all hidden reviews.
        /// </summary>
        /// <returns>A list of hidden reviews.</returns>
        List<Review> GetHiddenReviews();
    }
}
