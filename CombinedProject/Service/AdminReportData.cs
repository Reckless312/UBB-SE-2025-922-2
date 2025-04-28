// <copyright file="AdminReportData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using App1.Models;

/// <summary>
/// Represents a data structure containing administrative report information for a given date.
/// </summary>
public class AdminReportData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdminReportData"/> class.
    /// </summary>
    /// <param name="reportDate">The date the report was generated.</param>
    /// <param name="adminUsers">The list of administrative users.</param>
    /// <param name="activeUsersCount">The number of active users.</param>
    /// <param name="bannedUsersCount">The number of banned users.</param>
    /// <param name="newReviewsCount">The number of new reviews.</param>
    /// <param name="averageRating">The average user rating.</param>
    /// <param name="recentReviews">The list of recent reviews.</param>
    public AdminReportData(
        DateTime reportDate,
        List<User> adminUsers,
        int activeUsersCount,
        int bannedUsersCount,
        int newReviewsCount,
        double averageRating,
        List<Review> recentReviews)
    {
        this.ReportDate = reportDate;
        this.AdminUsers = adminUsers;
        this.ActiveUsersCount = activeUsersCount;
        this.BannedUsersCount = bannedUsersCount;
        this.NewReviewsCount = newReviewsCount;
        this.AverageRating = averageRating;
        this.RecentReviews = recentReviews;
    }

    /// <summary>
    /// Gets or sets the date the report was generated.
    /// </summary>
    public DateTime ReportDate { get; set; }

    /// <summary>
    /// Gets or sets the list of administrative users.
    /// </summary>
    public List<User> AdminUsers { get; set; }

    /// <summary>
    /// Gets or sets the number of active users.
    /// </summary>
    public int ActiveUsersCount { get; set; }

    /// <summary>
    /// Gets or sets the number of banned users.
    /// </summary>
    public int BannedUsersCount { get; set; }

    /// <summary>
    /// Gets or sets the number of new reviews submitted.
    /// </summary>
    public int NewReviewsCount { get; set; }

    /// <summary>
    /// Gets or sets the average rating across all reviews.
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the list of recent reviews.
    /// </summary>
    public List<Review> RecentReviews { get; set; }
}
