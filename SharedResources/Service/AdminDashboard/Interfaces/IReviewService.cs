// <copyright file="IReviewService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.Service.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    public interface IReviewService
    {
        public void HideReview(int reviewID);

        public List<Review> GetFlaggedReviews();

        public List<Review> GetHiddenReviews();

        public List<Review> GetAllReviews();

        List<Review> GetReviewsSince(DateTime date);

        double GetAverageRatingForVisibleReviews();

        List<Review> GetMostRecentReviews(int count);

        List<Review> GetReviewsForReport();

        int GetReviewCountAfterDate(DateTime date);

        List<Review> GetReviewsByUser(Guid userId);

        void ResetReviewFlags(int reviewId);

        public List<Review> FilterReviewsByContent(string content);

        // public List<Review> FilterReviewsByUser(string userFilter);
    }
}
