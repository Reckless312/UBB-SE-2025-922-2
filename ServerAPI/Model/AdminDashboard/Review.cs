// <copyright file="Review.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Model.AdminDashboard
{
    using System;

    /// <summary>
    /// Initializes a new instance of the Review class with specified parameters.
    /// </summary>
    /// <param name="reviewId">The unique identifier for the review.</param>
    /// <param name="userId">The ID of the user who wrote the review.</param>
    /// <param name="rating">The rating given (typically 1-5).</param>
    /// <param name="content">The text content of the review.</param>
    /// <param name="createdDate">The date and time when the review was created.</param>
    /// <param name="numberOfFlags">The number of times this review has been flagged for inappropriate content.</param>
    /// <param name="isHidden">Indicates whether the review is hidden from public view.</param>
    public class Review(int reviewId, Guid userId, int rating,
                 string content, DateTime createdDate, int numberOfFlags = 0,
                 bool isHidden = false)
    {
        public int ReviewId { get; } = reviewId;

        public Guid UserId { get; } = userId;

        public int Rating { get; set; } = rating;

        public string Content { get; set; } = content;

        public DateTime CreatedDate { get; } = createdDate;

        public int NumberOfFlags { get; set; } = numberOfFlags;

        public bool IsHidden { get; set; } = isHidden;
    }
}