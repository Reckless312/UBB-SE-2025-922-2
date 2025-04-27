// <copyright file="ReviewsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using App1.Models;
    using App1.Repositories;

    public class ReviewsService : IReviewService
    {
        private readonly IReviewsRepository reviewsRepository;

        public ReviewsService(IReviewsRepository reviewsRepository)
        {
            this.reviewsRepository = reviewsRepository;
        }

        public void ResetReviewFlags(int reviewId)
        {
            this.reviewsRepository.UpdateNumberOfFlagsForReview(reviewId, 0);
        }

        public void HideReview(int reviewId)
        {
            this.reviewsRepository.UpdateReviewVisibility(reviewId, true);
        }

        public List<Review> GetFlaggedReviews()
        {
            return this.reviewsRepository.GetAllReviews().Where(review => review.NumberOfFlags > 0).ToList();
        }

        public List<Review> GetHiddenReviews()
        {
            return this.reviewsRepository.GetAllReviews().Where(review => review.IsHidden == true).ToList();
        }

        public List<Review> GetAllReviews()
        {
            return this.reviewsRepository.GetAllReviews();
        }

        public List<Review> GetReviewsSince(DateTime date)
        {
            return this.reviewsRepository.GetReviewsSince(date);
        }

        public double GetAverageRatingForVisibleReviews()
        {
            return this.reviewsRepository.GetAverageRatingForVisibleReviews();
        }

        public List<Review> GetMostRecentReviews(int count)
        {
            return this.reviewsRepository.GetMostRecentReviews(count);
        }

        public int GetReviewCountAfterDate(DateTime date)
        {
            return this.reviewsRepository.GetReviewCountAfterDate(date);
        }

        public List<Review> GetReviewsByUser(int userId)
        {
            return this.reviewsRepository.GetReviewsByUser(userId);
        }

        public List<Review> GetReviewsForReport()
        {
            DateTime date = DateTime.Now.AddDays(-1);
            int count = this.reviewsRepository.GetReviewCountAfterDate(date);

            List<Review> reviews = this.reviewsRepository.GetMostRecentReviews(count);
            return reviews??[];
        }

        public List<Review> FilterReviewsByContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return this.GetFlaggedReviews();
            }

            content = content.ToLower();
            return this.GetFlaggedReviews()
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