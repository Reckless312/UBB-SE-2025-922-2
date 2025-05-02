// <copyright file="ReviewsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Repository.Authentication;
    using IRepository;
    using Microsoft.Data.SqlClient;
    public class ReviewsRepository : IReviewsRepository
    {
        public List<Review> GetAllReviews()
        {
            List<Review> reviews = new();
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT * FROM Reviews", connection);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                reviews.Add(ReadReview(reader));
            }

            return reviews;
        }


        public List<Review> GetReviewsSince(DateTime date)
        {
            List<Review> reviews = new();
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT * FROM Reviews WHERE CreatedDate >= @date AND IsHidden = 0 ORDER BY CreatedDate DESC", connection);
            cmd.Parameters.Add("@date", SqlDbType.DateTime2).Value = date;
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                reviews.Add(ReadReview(reader));
            }

            return reviews;
        }

        public double GetAverageRatingForVisibleReviews()
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews WHERE IsHidden = 0", connection);
            object result = cmd.ExecuteScalar();

            return result == DBNull.Value ? 0.0 : Math.Round(Convert.ToDouble(result), 1);
        }

        public List<Review> GetMostRecentReviews(int count)
        {
            List<Review> reviews = new();
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT TOP (@count) * FROM Reviews WHERE IsHidden = 0 ORDER BY CreatedDate DESC", connection);
            cmd.Parameters.Add("@count", SqlDbType.Int).Value = count;
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                reviews.Add(ReadReview(reader));
            }

            return reviews;
        }

        public int GetReviewCountAfterDate(DateTime date)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT COUNT(*) FROM Reviews WHERE CreatedDate >= @date AND IsHidden = 0", connection);
            cmd.Parameters.Add("@date", SqlDbType.DateTime2).Value = date;
            return (int)cmd.ExecuteScalar();
        }

        public List<Review> GetFlaggedReviews(int minFlags)
        {
            List<Review> reviews = new();
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT * FROM Reviews WHERE NumberOfFlags >= @minFlags AND IsHidden = 0", connection);
            cmd.Parameters.Add("@minFlags", SqlDbType.Int).Value = minFlags;
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                reviews.Add(ReadReview(reader));
            }

            return reviews;
        }

        public List<Review> GetReviewsByUser(Guid userId)
        {
            List<Review> reviews = new();
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT * FROM Reviews WHERE UserId = @userId AND IsHidden = 0 ORDER BY CreatedDate DESC", connection);
            cmd.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = userId;
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                reviews.Add(ReadReview(reader));
            }

            return reviews;
        }

        public Review GetReviewById(int reviewId)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT * FROM Reviews WHERE ReviewId = @reviewId", connection);
            cmd.Parameters.Add("@reviewId", SqlDbType.Int).Value = reviewId;
            using SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return ReadReview(reader);
            }

            return null;
        }

        public void UpdateReviewVisibility(int reviewId, bool isHidden)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("UPDATE Reviews SET IsHidden = @isHidden WHERE ReviewId = @reviewId", connection);
            cmd.Parameters.Add("@isHidden", SqlDbType.Bit).Value = isHidden;
            cmd.Parameters.Add("@reviewId", SqlDbType.Int).Value = reviewId;
            cmd.ExecuteNonQuery();
        }

        public void UpdateNumberOfFlagsForReview(int reviewId, int numberOfFlags)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("UPDATE Reviews SET NumberOfFlags = @numberOfFlags WHERE ReviewId = @reviewId", connection);
            cmd.Parameters.Add("@numberOfFlags", SqlDbType.Int).Value = numberOfFlags;
            cmd.Parameters.Add("@reviewId", SqlDbType.Int).Value = reviewId;
            cmd.ExecuteNonQuery();
        }

        public int AddReview(Review review)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("INSERT INTO Reviews (UserId, Rating, Content, CreatedDate, NumberOfFlags, IsHidden) OUTPUT INSERTED.ReviewId VALUES (@userId, @rating, @content, @createdDate, @numberOfFlags, @isHidden)", connection);

            cmd.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = review.UserId;
            cmd.Parameters.Add("@rating", SqlDbType.Int).Value = review.Rating;
            cmd.Parameters.Add("@content", SqlDbType.NVarChar).Value = review.Content;
            cmd.Parameters.Add("@createdDate", SqlDbType.DateTime2).Value = review.CreatedDate;
            cmd.Parameters.Add("@numberOfFlags", SqlDbType.Int).Value = review.NumberOfFlags;
            cmd.Parameters.Add("@isHidden", SqlDbType.Bit).Value = review.IsHidden;

            return (int)cmd.ExecuteScalar();
        }

        public bool RemoveReviewById(int reviewId)
        {
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("DELETE FROM Reviews WHERE ReviewId = @reviewId", connection);
            cmd.Parameters.Add("@reviewId", SqlDbType.Int).Value = reviewId;
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Review> GetHiddenReviews()
        {
            List<Review> reviews = new();
            using SqlConnection connection = DrinkDbConnectionHelper.GetConnection();
            using SqlCommand cmd = new("SELECT * FROM Reviews WHERE IsHidden = 1", connection);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                reviews.Add(ReadReview(reader));
            }

            return reviews;
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