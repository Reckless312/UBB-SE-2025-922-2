// <copyright file="Review.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.Model.AdminDashboard
{
    using System;

    public class Review
    {
        public Review() { }

        public Review(int reviewId, Guid userId, int rating,
                 string content, DateTime createdDate, int numberOfFlags = 0,
                 bool isHidden = false)
        {
            ReviewId = reviewId;
            UserId = userId;
            Rating = rating;
            Content = content;
            CreatedDate = createdDate;
            NumberOfFlags = numberOfFlags;
            IsHidden = isHidden;
        }
        public int ReviewId { get; set; }

        public Guid UserId { get; set; }

        public int Rating { get; set; } 

        public string Content { get; set; } 

        public DateTime CreatedDate { get; set; }

        public int NumberOfFlags { get; set; }

        public bool IsHidden { get; set; }
    }
}