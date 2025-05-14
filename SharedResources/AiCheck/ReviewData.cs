// <copyright file="ReviewData.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DataAccess.AiCheck
{
    using Microsoft.ML.Data;

    public class ReviewData
    {
        [LoadColumn(0)]
        [ColumnName("ReviewContent")]
        public string ReviewContent { get; set; }

        [LoadColumn(1)]
        [ColumnName("IsOffensiveContent")]
        public bool IsOffensiveContent { get; set; }
    }
}