// <copyright file="ReviewData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.AiCheck
{
    using Microsoft.ML.Data;

    /// <summary>
    /// Represents a review data point for training the offensive content detection model.
    /// </summary>
    public class ReviewData
    {
        /// <summary>
        /// Gets or sets the text content of the review.
        /// </summary>
        [LoadColumn(0)]
        [ColumnName("ReviewContent")]
        public string ReviewContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the review contains offensive content.
        /// </summary>
        [LoadColumn(1)]
        [ColumnName("IsOffensiveContent")]
        public bool IsOffensiveContent { get; set; }
    }
}