// <copyright file="IReviewService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    /// <summary>
    /// Service interface for managing reviews in the application.
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Adds a new review to the system.
        /// </summary>
        /// <param name="review">The review to add.</param>
        /// <returns>The ID of the newly added review.</returns>
        Task<int> AddReview(Review review);

        /// <summary>
        /// Removes a review by its ID.
        /// </summary>
        /// <param name="reviewId">The ID of the review to remove.</param>
        Task RemoveReviewById(int reviewId);

        /// <summary>
        /// Gets a specific review by its ID.
        /// </summary>
        /// <param name="reviewId">The ID of the review to retrieve.</param>
        /// <returns>The review matching the specified ID.</returns>
        Task<Review> GetReviewById(int reviewId);

        /// <summary>
        /// Updates the number of flags for a specific review.
        /// </summary>
        /// <param name="reviewId">The ID of the review to update.</param>
        /// <param name="numberOfFlags">The new number of flags.</param>
        Task UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags);

        /// <summary>
        /// Updates the visibility status of a review.
        /// </summary>
        /// <param name="reviewId">The ID of the review to update.</param>
        /// <param name="isHidden">Whether the review should be hidden.</param>
        Task UpdateReviewVisibility(int reviewId, bool isHidden);

        /// <summary>
        /// Hides a review from public view.
        /// </summary>
        /// <param name="reviewID">The ID of the review to hide.</param>
        Task HideReview(int reviewID);

        /// <summary>
        /// Gets all reviews that have been flagged for inappropriate content.
        /// </summary>
        /// <param name="minFlags">The minimum number of flags a review should have.</param>
        /// <returns>A list of flagged reviews.</returns>
        Task<List<Review>> GetFlaggedReviews(int minFlags = 1);

        /// <summary>
        /// Gets all reviews that are currently hidden from public view.
        /// </summary>
        /// <returns>A list of hidden reviews.</returns>
        Task<List<Review>> GetHiddenReviews();

        /// <summary>
        /// Gets all reviews in the system.
        /// </summary>
        /// <returns>A list of all reviews.</returns>
        Task<List<Review>> GetAllReviews();

        /// <summary>
        /// Gets reviews created since a specific date.
        /// </summary>
        /// <param name="date">The date to filter reviews from.</param>
        /// <returns>A list of reviews created after the specified date.</returns>
        Task<List<Review>> GetReviewsSince(DateTime date);

        /// <summary>
        /// Calculates the average rating of all visible reviews.
        /// </summary>
        /// <returns>The average rating as a double value.</returns>
        Task<double> GetAverageRatingForVisibleReviews();

        /// <summary>
        /// Gets the most recent reviews.
        /// </summary>
        /// <param name="count">The number of reviews to retrieve.</param>
        /// <returns>A list of the most recent reviews, limited to the specified count.</returns>
        Task<List<Review>> GetMostRecentReviews(int count);

        /// <summary>
        /// Gets reviews for reporting purposes.
        /// </summary>
        /// <returns>A list of reviews formatted for reporting.</returns>
        Task<List<Review>> GetReviewsForReport();

        /// <summary>
        /// Gets the count of reviews created after a specific date.
        /// </summary>
        /// <param name="date">The date to count reviews from.</param>
        /// <returns>The number of reviews created after the specified date.</returns>
        Task<int> GetReviewCountAfterDate(DateTime date);

        /// <summary>
        /// Gets all reviews created by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of reviews created by the specified user.</returns>
        Task<List<Review>> GetReviewsByUser(Guid userId);

        /// <summary>
        /// Resets the flag count for a review to zero.
        /// </summary>
        /// <param name="reviewId">The ID of the user who created the review.</param>
        Task ResetReviewFlags(int reviewId);

        // Add new filter methods
        Task<List<Review>> FilterReviewsByContent(string content);

        // Task<List<Review>> FilterReviewsByUser(string userFilter);
    }
}
