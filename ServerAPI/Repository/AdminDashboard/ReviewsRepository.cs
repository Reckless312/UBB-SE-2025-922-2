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
    using ServerAPI.Data;
    using Microsoft.EntityFrameworkCore;
    using static Repository.AdminDashboard.UserRepository;

    public class ReviewsRepository : IReviewsRepository
    {
        private readonly DatabaseContext _context;

        public ReviewsRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task LoadReviews(IEnumerable<Review> reviewsToLoad)
        {
            try
            {
                if (reviewsToLoad == null)
                {
                    throw new ArgumentNullException(nameof(reviewsToLoad));
                }

                await _context.Reviews.AddRangeAsync(reviewsToLoad);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to load reviews.", ex);
            }
        }

        public async Task<List<Review>> GetAllReviews()
        {
            return  _context.Reviews.ToListAsync().Result;
        }

        public async Task<List<Review>> GetReviewsSince(DateTime date)
        {
            try
            {
                return await _context.Reviews
                    .Where(review => review.CreatedDate >= date && !review.IsHidden)
                    .OrderByDescending(review => review.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve reviews since {date}.", ex);
            }
        }

        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            try
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
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to calculate the average rating for visible reviews.", ex);
            }
        }

        public async Task<List<Review>> GetMostRecentReviews(int count)
        {
            try
            {
                return await _context.Reviews
                    .Where(review => !review.IsHidden)
                    .OrderByDescending(review => review.CreatedDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve the most recent {count} reviews.", ex);
            }
        }

        public async Task<int> GetReviewCountAfterDate(DateTime date)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(review => review.CreatedDate >= date && !review.IsHidden);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to count reviews after {date}.", ex);
            }
        }

        public async Task<List<Review>> GetFlaggedReviews(int minFlags)
        {
            try
            {
                return _context.Reviews
                    .Where(review => review.NumberOfFlags >= minFlags && !review.IsHidden)
                    .ToListAsync().Result;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve flagged reviews with at least {minFlags} flags.", ex);
            }
        }

        public async Task<List<Review>> GetReviewsByUser(Guid userId)
        {
            try
            {
                return await _context.Reviews
                    .Where(review => review.UserId == userId && !review.IsHidden)
                    .OrderByDescending(review => review.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve reviews for user with ID {userId}.", ex);
            }
        }

        public async Task<Review> GetReviewById(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);

                if (review == null)
                {
                    throw new ArgumentException($"No review found with ID {reviewId}");
                }

                return review;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve review with ID {reviewId}.", ex);
            }
        }

        public async Task UpdateReviewVisibility(int reviewId, bool isHidden)
        {
            try
            {
                var review = await GetReviewById(reviewId);
                review.IsHidden = isHidden;
                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to update visibility for review with ID {reviewId}.", ex);
            }
        }

        public async Task UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags)
        {
            try
            {
                var review = await GetReviewById(reviewId);
                review.NumberOfFlags = numberOfFlags;
                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to update flags for review with ID {reviewId}.", ex);
            }
        }

        public async Task<int> AddReview(Review review)
        {
            try
            {
                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();
                return review.ReviewId;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to add a new review.", ex);
            }
        }

        public async Task<bool> RemoveReviewById(int reviewId)
        {
            try
            {
                var review = await GetReviewById(reviewId);
                _context.Reviews.Remove(review);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to remove review with ID {reviewId}.", ex);
            }
        }

        public async Task<List<Review>> GetHiddenReviews()
        {
            try
            {
                return await _context.Reviews
                    .Where(review => review.IsHidden)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve hidden reviews.", ex);
            }
        }
    }
}