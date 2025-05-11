// <copyright file="ReviewsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
            return reviewsRepository.GetFlaggedReviews(1).Result;
        }

        public List<Review> GetHiddenReviews()
        {

            List<Review> reviews = reviewsRepository.GetAllReviews().Result;
            return reviews.Where(review => review.IsHidden == true).ToList();
        }

        public List<Review> GetAllReviews()
        {
            return  reviewsRepository.GetAllReviews().Result;
        }

        public List<Review> GetReviewsSince(DateTime date)
        {
            return reviewsRepository.GetReviewsSince(date).Result;
        }

        public double GetAverageRatingForVisibleReviews()
        {
            return reviewsRepository.GetAverageRatingForVisibleReviews().Result;
        }

        public List<Review> GetMostRecentReviews(int count)
        {
            return reviewsRepository.GetMostRecentReviews(count).Result;
        }

        public int GetReviewCountAfterDate(DateTime date)
        {
            return reviewsRepository.GetReviewCountAfterDate(date).Result;
        }

        public List<Review> GetReviewsByUser(Guid userId)
        {
            return reviewsRepository.GetReviewsByUser(userId).Result;
        }

        public List<Review> GetReviewsForReport()
        {
            DateTime date = DateTime.Now.AddDays(-1);
            int count = reviewsRepository.GetReviewCountAfterDate(date).Result;

            List<Review> reviews = reviewsRepository.GetMostRecentReviews(count).Result;
            return reviews ?? [];
        }

        public List<Review> FilterReviewsByContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return GetAllReviews();
            }

            content = content.ToLower();
            List<Review> reviews = GetAllReviews();
            return reviews.Where(review => review.Content.ToLower().Contains(content)).ToList();
        }
    }
}