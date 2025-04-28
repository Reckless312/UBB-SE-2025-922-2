// <copyright file="ReviewsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using App1.Models;

    public class ReviewsRepository : IReviewsRepository
    {
        private readonly List<Review> reviews;
        private int nextReviewId;

        public ReviewsRepository()
        {
            this.reviews = new List<Review>();
            this.nextReviewId = 1;
        }

        public void LoadReviews(IEnumerable<Review> reviewsToLoad)
        {
            if (reviewsToLoad == null)
            {
                throw new ArgumentNullException(nameof(reviewsToLoad));
            }

            foreach (var review in reviewsToLoad)
            {
                // Ensure we maintain ID sequence by using the AddReview method
                this.AddReview(review);
            }
        }

        public List<Review> GetAllReviews()
        {
            return this.reviews.ToList();
        }

        public List<Review> GetReviewsSince(DateTime date)
        {
            return this.reviews
                .Where(review => review.CreatedDate >= date && !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .ToList();
        }

        public double GetAverageRatingForVisibleReviews()
        {
            if (!this.reviews.Any(review => !review.IsHidden))
            {
                return 0.0;
            }

            double average = this.reviews
                .Where(review => !review.IsHidden)
                .Average(r => r.Rating);
            return Math.Round(average, 1);
        }

        public List<Review> GetMostRecentReviews(int count)
        {
            return this.reviews
                .Where(review => !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .Take(count)
                .ToList();
        }

        public int GetReviewCountAfterDate(DateTime date)
        {
            return this.reviews
                .Count(review => review.CreatedDate >= date && !review.IsHidden);
        }

        public List<Review> GetFlaggedReviews(int minFlags)
        {
            return this.reviews
                .Where(review => review.NumberOfFlags >= minFlags && !review.IsHidden)
                .ToList();
        }

        public List<Review> GetReviewsByUser(int userId)
        {
            return this.reviews.Where(review => review.UserId == userId && !review.IsHidden).OrderByDescending(review => review.CreatedDate).ToList();
        }

        public Review GetReviewById(int reviewId)
        {
            return this.reviews.FirstOrDefault(review => review.ReviewId == reviewId);
        }

        public void UpdateReviewVisibility(int reviewId, bool isHidden)
        {
            Review? currentReview = this.reviews.FirstOrDefault(review => review.ReviewId == reviewId);

            if (currentReview != null)
            {
                currentReview.IsHidden = isHidden;
            }
        }

        public void UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags)
        {
            Review? currentReview = this.reviews.FirstOrDefault(review => review.ReviewId == reviewId);
            if (currentReview != null)
            {
                currentReview.NumberOfFlags = numberOfFlags;
            }
        }

        public int AddReview(Review review)
        {
            // Normally, this would be handled by the database
            int newId = this.nextReviewId++;
            Review newReview = new Review(
                reviewId: newId,
                userId: review.UserId,
                rating: review.Rating,
                content: review.Content,
                createdDate: review.CreatedDate,
                numberOfFlags: review.NumberOfFlags,
                isHidden: review.IsHidden);
            this.reviews.Add(newReview);
            return newId;
        }

        public bool RemoveReviewById(int reviewId)
        {
            Review? reviewToRemove = this.reviews.FirstOrDefault(review => review.ReviewId == reviewId);
            if (reviewToRemove != null)
            {
                this.reviews.Remove(reviewToRemove);
                return true;
            }

            return false;
        }

        public List<Review> GetHiddenReviews()
        {
            return this.reviews.Where(review => review.IsHidden).ToList();
        }
    }
}