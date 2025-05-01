// <copyright file="ReviewsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using IRepository;

    public class ReviewsService : IReviewService
    {
        private readonly IReviewsRepository reviewsRepository;

        public ReviewsService(IReviewsRepository reviewsRepository)
        {
            this.reviewsRepository = reviewsRepository;
        }

        public void ResetReviewFlags(int reviewId)
        {
            reviewsRepository.UpdateNumberOfFlagsForReview(reviewId, 0);
        }

        public void HideReview(int reviewId)
        {
            reviewsRepository.UpdateReviewVisibility(reviewId, true);
        }

        public List<Review> GetFlaggedReviews()
        {
            return reviewsRepository.GetAllReviews().Where(review => review.NumberOfFlags > 0).ToList();
        }

        public List<Review> GetHiddenReviews()
        {
            return reviewsRepository.GetAllReviews().Where(review => review.IsHidden == true).ToList();
        }

        public List<Review> GetAllReviews()
        {
            return reviewsRepository.GetAllReviews();
        }

        public List<Review> GetReviewsSince(DateTime date)
        {
            return reviewsRepository.GetReviewsSince(date);
        }

        public double GetAverageRatingForVisibleReviews()
        {
            return reviewsRepository.GetAverageRatingForVisibleReviews();
        }

        public List<Review> GetMostRecentReviews(int count)
        {
            return reviewsRepository.GetMostRecentReviews(count);
        }

        public int GetReviewCountAfterDate(DateTime date)
        {
            return reviewsRepository.GetReviewCountAfterDate(date);
        }

        public List<Review> GetReviewsByUser(Guid userId)
        {
            return reviewsRepository.GetReviewsByUser(userId);
        }

        public List<Review> GetReviewsForReport()
        {
            DateTime date = DateTime.Now.AddDays(-1);
            int count = reviewsRepository.GetReviewCountAfterDate(date);

            List<Review> reviews = reviewsRepository.GetMostRecentReviews(count);
            return reviews??[];
        }

        public List<Review> FilterReviewsByContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return GetFlaggedReviews();
            }

            content = content.ToLower();
            return GetFlaggedReviews()
                .Where(review => review.Content.ToLower().Contains(content))
                .ToList();
        }

        /*
        public List<Review> FilterReviewsByUser(string userFilter)
        {
            if (string.IsNullOrEmpty(userFilter))
            {
                return GetFlaggedReviews();
            }

            userFilter = userFilter.ToLower();
            return GetFlaggedReviews()
                .Where(review => review.UserName.ToLower().Contains(userFilter))
                .ToList();
        }
        */
    }
}