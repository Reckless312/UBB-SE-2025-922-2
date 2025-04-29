// <copyright file="ReviewPrediction.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.AiCheck
{
    using Microsoft.ML.Data;

    /// <summary>
    /// Represents the prediction output from the offensive content detection model.
    /// </summary>
    public class ReviewPrediction
    {
        /// <summary>
        /// Gets or sets a value indicating whether the predicted classification (true = offensive, false = not offensive).
        /// </summary>
        [ColumnName("PredictedLabel")]
        public bool IsPredictedOffensive { get; set; }

        /// <summary>
        /// Gets or sets the probability score indicating the confidence of the offensive content prediction.
        /// </summary>
        [ColumnName("Score")]
        public float OffensiveProbabilityScore { get; set; }
    }
}