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

        public async Task ResetReviewFlags(int reviewId)
        {
            await reviewsRepository.UpdateNumberOfFlagsForReview(reviewId, 0);
        }

        public async Task HideReview(int reviewId)
        {
             await reviewsRepository.UpdateReviewVisibility(reviewId, true);
        }

        public async Task<List<Review>> GetFlaggedReviews()
        {
            List<Review> reviews = await reviewsRepository.GetAllReviews();
            return reviews.Where(review => review.NumberOfFlags > 0).ToList();
        }

        public async Task<List<Review>> GetHiddenReviews()
        {

            List<Review> reviews = await reviewsRepository.GetAllReviews();
            return reviews.Where(review => review.IsHidden == true).ToList();
        }

        public async Task<List<Review>> GetAllReviews()
        {
            return await reviewsRepository.GetAllReviews();
        }

        public async Task<List<Review>> GetReviewsSince(DateTime date)
        {
            return await reviewsRepository.GetReviewsSince(date);
        }

        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            return await reviewsRepository.GetAverageRatingForVisibleReviews();
        }

        public async Task<List<Review>> GetMostRecentReviews(int count)
        {
            return await reviewsRepository.GetMostRecentReviews(count);
        }

        public async Task<int> GetReviewCountAfterDate(DateTime date)
        {
            return await reviewsRepository.GetReviewCountAfterDate(date);
        }

        public async Task<List<Review>> GetReviewsByUser(Guid userId)
        {
            return await reviewsRepository.GetReviewsByUser(userId);
        }

        public async Task<List<Review>> GetReviewsForReport()
        {
            DateTime date = DateTime.Now.AddDays(-1);
            int count = await reviewsRepository.GetReviewCountAfterDate(date);

            List<Review> reviews = await reviewsRepository.GetMostRecentReviews(count);
            return reviews ?? [];
        }

        public async Task<List<Review>> FilterReviewsByContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return await GetFlaggedReviews();
            }

            content = content.ToLower();
            List<Review> reviews = await GetFlaggedReviews();
            return reviews.Where(review => review.Content.ToLower().Contains(content)).ToList();
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