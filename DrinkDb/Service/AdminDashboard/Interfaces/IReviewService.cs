// <copyright file="IReviewService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using SharedResources.Model.AdminDashboard;

    /// <summary>
    /// Service interface for managing reviews in the application.
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Hides a review from public view.
        /// </summary>
        /// <param name="reviewID">The ID of the review to hide.</param>
        public void HideReview(int reviewID);

        /// <summary>
        /// Gets all reviews that have been flagged for inappropriate content.
        /// </summary>
        /// <returns>A list of flagged reviews.</returns>
        public List<Review> GetFlaggedReviews();

        /// <summary>
        /// Gets all reviews that are currently hidden from public view.
        /// </summary>
        /// <returns>A list of hidden reviews.</returns>
        public List<Review> GetHiddenReviews();

        /// <summary>
        /// Gets all reviews in the system.
        /// </summary>
        /// <returns>A list of all reviews.</returns>
        public List<Review> GetAllReviews();

        /// <summary>
        /// Gets reviews created since a specific date.
        /// </summary>
        /// <param name="date">The date to filter reviews from.</param>
        /// <returns>A list of reviews created after the specified date.</returns>
        List<Review> GetReviewsSince(DateTime date);

        /// <summary>
        /// Calculates the average rating of all visible reviews.
        /// </summary>
        /// <returns>The average rating as a double value.</returns>
        double GetAverageRatingForVisibleReviews();

        /// <summary>
        /// Gets the most recent reviews.
        /// </summary>
        /// <param name="count">The number of reviews to retrieve.</param>
        /// <returns>A list of the most recent reviews, limited to the specified count.</returns>
        List<Review> GetMostRecentReviews(int count);

        /// <summary>
        /// Gets reviews for reporting purposes.
        /// </summary>
        /// <returns>A list of reviews formatted for reporting.</returns>
        List<Review> GetReviewsForReport();

        /// <summary>
        /// Gets the count of reviews created after a specific date.
        /// </summary>
        /// <param name="date">The date to count reviews from.</param>
        /// <returns>The number of reviews created after the specified date.</returns>
        int GetReviewCountAfterDate(DateTime date);

        /// <summary>
        /// Gets all reviews created by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of reviews created by the specified user.</returns>
        List<Review> GetReviewsByUser(Guid userId);

        /// <summary>
        /// Resets the flag count for a review to zero.
        /// </summary>
        /// <param name="reviewId">The ID of the user who created the review.</param>
        void ResetReviewFlags(int reviewId);

        // Add new filter methods
        public List<Review> FilterReviewsByContent(string content);

        // public List<Review> FilterReviewsByUser(string userFilter);
    }
}
