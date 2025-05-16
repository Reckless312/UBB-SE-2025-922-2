// <copyright file="ReviewsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.Service.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using IRepository;

    public class ReviewsService : IReviewService
    {
        private readonly IReviewsRepository reviewsRepository;

        public ReviewsService(IReviewsRepository reviewsRepository)
        {
            this.reviewsRepository = reviewsRepository;
        }

        public Task<int> AddReview(Review review)
        {
            return reviewsRepository.AddReview(review);
        }

        public Task RemoveReviewById(int reviewId)
        {
            return reviewsRepository.RemoveReviewById(reviewId);
        }

        public Task<Review> GetReviewById(int reviewId)
        {
            return reviewsRepository.GetReviewById(reviewId);
        }

        public Task UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags)
        {
            return reviewsRepository.UpdateNumberOfFlagsForReview(reviewId, numberOfFlags);
        }

        public Task UpdateReviewVisibility(int reviewId, bool isHidden)
        {
            return reviewsRepository.UpdateReviewVisibility(reviewId, isHidden);
        }

        public void ResetReviewFlags(int reviewId)
        {
            reviewsRepository.UpdateNumberOfFlagsForReview(reviewId, 0);
        }

        public void HideReview(int reviewId)
        {
             reviewsRepository.UpdateReviewVisibility(reviewId, true);
        }

        public List<Review> GetFlaggedReviews(int minFlags = 1)
        {
            return reviewsRepository.GetFlaggedReviews(minFlags).Result;
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
                return GetFlaggedReviews();
            }

            content = content.ToLower();
            List<Review> reviews = GetFlaggedReviews();
            return reviews.Where(review => review.Content.ToLower().Contains(content)).ToList();
        }

        public List<Review> GetFlaggedReviews()
        {
            throw new NotImplementedException();
        }
    }
}