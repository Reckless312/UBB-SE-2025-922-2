// <copyright file="ReviewsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using DataAccess.Model.AdminDashboard;
using System;
using Xunit;

namespace UnitTests.Reviews
{
    public class ReviewsTests
    {
        [Fact]
        public void Review_Constructor_SetsAllProperties()
        {
            // Arrange
            int reviewId = 1;
            Guid userId = Guid.NewGuid();
            int rating = 4;
            string content = "Great drink!";
            DateTime createdDate = DateTime.Now;
            int numberOfFlags = 0;
            bool isHidden = false;

            // Act
            Review review = new Review(
                reviewId: reviewId,
                userId: userId,
                rating: rating,
                content: content,
                createdDate: createdDate,
                numberOfFlags: numberOfFlags,
                isHidden: isHidden);

            // Assert
            Assert.Equal(reviewId, review.ReviewId);
            Assert.Equal(userId, review.UserId);
            Assert.Equal(rating, review.Rating);
            Assert.Equal(content, review.Content);
            Assert.Equal(createdDate, review.CreatedDate);
            Assert.Equal(numberOfFlags, review.NumberOfFlags);
            Assert.Equal(isHidden, review.IsHidden);
        }

        [Fact]
        public void Review_ModifyRating_UpdatesRating()
        {
            // Arrange
            Review review = new Review(
                reviewId: 1,
                userId: Guid.NewGuid(),
                rating: 4,
                content: "Great drink!",
                createdDate: DateTime.Now);
            int newRating = 5;

            // Act
            review.Rating = newRating;

            // Assert
            Assert.Equal(newRating, review.Rating);
        }

        [Fact]
        public void Review_ModifyContent_UpdatesContent()
        {
            // Arrange
            Review review = new Review(
                reviewId: 1,
                userId: Guid.NewGuid(),
                rating: 4,
                content: "Great drink!",
                createdDate: DateTime.Now);
            string newContent = "Amazing drink!";

            // Act
            review.Content = newContent;

            // Assert
            Assert.Equal(newContent, review.Content);
        }

        [Fact]
        public void Review_ModifyNumberOfFlags_UpdatesNumberOfFlags()
        {
            // Arrange
            Review review = new Review(
                reviewId: 1,
                userId: Guid.NewGuid(),
                rating: 4,
                content: "Great drink!",
                createdDate: DateTime.Now,
                numberOfFlags: 0);
            int newNumberOfFlags = 2;

            // Act
            review.NumberOfFlags = newNumberOfFlags;

            // Assert
            Assert.Equal(newNumberOfFlags, review.NumberOfFlags);
        }

        [Fact]
        public void Review_ModifyIsHidden_UpdatesIsHidden()
        {
            // Arrange
            Review review = new Review(
                reviewId: 1,
                userId: Guid.NewGuid(),
                rating: 4,
                content: "Great drink!",
                createdDate: DateTime.Now,
                isHidden: false);
            bool newIsHidden = true;

            // Act
            review.IsHidden = newIsHidden;

            // Assert
            Assert.Equal(newIsHidden, review.IsHidden);
        }
    }
}
