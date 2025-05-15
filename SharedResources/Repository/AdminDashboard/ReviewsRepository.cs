// <copyright file="ReviewsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using static Repository.AdminDashboard.UserRepository;
    using IRepository;
    using Microsoft.Data.SqlClient;
    using Repository.Authentication;
    public class ReviewsRepository : IReviewsRepository
    {
        private readonly DatabaseContext _context;

        public ReviewsRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task LoadReviews(IEnumerable<Review> reviewsToLoad)
        {


            await _context.Reviews.AddRangeAsync(reviewsToLoad);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Review>> GetAllReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        public async Task<List<Review>> GetReviewsSince(DateTime date)
        {
            return await _context.Reviews
                .Where(review => review.CreatedDate >= date && !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            var visibleReviews = await _context.Reviews
                .Where(review => !review.IsHidden)
                .ToListAsync();

            if (!visibleReviews.Any())
            {
                return 0.0;
            }
            return Math.Round(visibleReviews.Average(r => r.Rating), 1);
        }

        public async Task<List<Review>> GetMostRecentReviews(int count)
        {
            return await _context.Reviews
                .Where(review => !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetReviewCountAfterDate(DateTime date)
        {
            return await _context.Reviews
                .CountAsync(review => review.CreatedDate >= date && !review.IsHidden);
        }

        public async Task<List<Review>> GetFlaggedReviews(int minFlags)
        {
            return await _context.Reviews
                .Where(review => review.NumberOfFlags >= minFlags && !review.IsHidden)
                .ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUser(Guid userId)
        {
            return await _context.Reviews
                .Where(review => review.UserId == userId && !review.IsHidden)
                .OrderByDescending(review => review.CreatedDate)
                .ToListAsync();
        }

        public async Task<Review> GetReviewById(int reviewId)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task UpdateReviewVisibility(int reviewId, bool isHidden)
        {

            var review = _context.Reviews.Find(reviewId);
            review.IsHidden = isHidden;
            _context.Reviews.Update(review);
            _context.SaveChanges();
        }

        public async Task UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags)
        {
            Review review = _context.Reviews.Find(reviewId);
            review.NumberOfFlags = numberOfFlags;
            _context.Reviews.Update(review);
            _context.SaveChanges();
        }

        public async Task<int> AddReview(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review.ReviewId;
        }

        public async Task RemoveReviewById(int reviewId)
        {
            var review = GetReviewById(reviewId).Result;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Review>> GetHiddenReviews()
        {
            return await _context.Reviews
                .Where(review => review.IsHidden)
                .ToListAsync();
        }

        private Review ReadReview(SqlDataReader reader)
        {
            return new Review(
                reviewId: reader.GetInt32(reader.GetOrdinal("ReviewId")),
                userId: reader.GetGuid(reader.GetOrdinal("UserId")),
                rating: reader.GetInt32(reader.GetOrdinal("Rating")),
                content: reader.GetString(reader.GetOrdinal("Content")),
                createdDate: reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                numberOfFlags: reader.GetInt32(reader.GetOrdinal("NumberOfFlags")),
                isHidden: reader.GetBoolean(reader.GetOrdinal("IsHidden"))
            );
        }

    }
}