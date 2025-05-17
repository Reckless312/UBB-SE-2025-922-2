namespace UnitTests.AiCheck
{
    using DataAccess.AiCheck;
    using System.Linq;
    using Xunit;

    public class ReviewFilterTests
    {
        [Fact]
        public void ReviewData_Constructor_SetsAllProperties()
        {
            string reviewContent = "This is a great drink!";
            bool isOffensiveContent = false;
            ReviewData reviewData = new ReviewData
            {
                ReviewContent = reviewContent,
                IsOffensiveContent = isOffensiveContent,
            };
            Assert.Equal(reviewContent, reviewData.ReviewContent);
            Assert.Equal(isOffensiveContent, reviewData.IsOffensiveContent);
        }

        [Fact]
        public void ReviewData_HasLoadColumnAttribute()
        {
            System.Reflection.PropertyInfo? reviewContentProperty = typeof(ReviewData).GetProperty("ReviewContent");
            System.Reflection.PropertyInfo? isOffensiveContentProperty = typeof(ReviewData).GetProperty("IsOffensiveContent");
            Assert.NotNull(reviewContentProperty);
            Assert.NotNull(isOffensiveContentProperty);
            object? reviewContentAttribute = reviewContentProperty.GetCustomAttributes(typeof(Microsoft.ML.Data.LoadColumnAttribute), false).FirstOrDefault();
            object? isOffensiveContentAttribute = isOffensiveContentProperty.GetCustomAttributes(typeof(Microsoft.ML.Data.LoadColumnAttribute), false).FirstOrDefault();
            Assert.NotNull(reviewContentAttribute);
            Assert.NotNull(isOffensiveContentAttribute);
        }

        [Fact]
        public void ReviewData_HasColumnNameAttribute()
        {
            System.Reflection.PropertyInfo? reviewContentProperty = typeof(ReviewData).GetProperty("ReviewContent");
            System.Reflection.PropertyInfo? isOffensiveContentProperty = typeof(ReviewData).GetProperty("IsOffensiveContent");
            Assert.NotNull(reviewContentProperty);
            Assert.NotNull(isOffensiveContentProperty);
            object? reviewContentAttribute = reviewContentProperty.GetCustomAttributes(typeof(Microsoft.ML.Data.ColumnNameAttribute), false).FirstOrDefault();
            object? isOffensiveContentAttribute = isOffensiveContentProperty.GetCustomAttributes(typeof(Microsoft.ML.Data.ColumnNameAttribute), false).FirstOrDefault();
            Assert.NotNull(reviewContentAttribute);
            Assert.NotNull(isOffensiveContentAttribute);
        }

        [Fact]
        public void ReviewPrediction_Constructor_SetsAllProperties()
        {
            bool isPredictedOffensive = true;
            float offensiveProbabilityScore = 0.85f;
            ReviewPrediction reviewPrediction = new ReviewPrediction
            {
                IsPredictedOffensive = isPredictedOffensive,
                OffensiveProbabilityScore = offensiveProbabilityScore,
            };
            Assert.Equal(isPredictedOffensive, reviewPrediction.IsPredictedOffensive);
            Assert.Equal(offensiveProbabilityScore, reviewPrediction.OffensiveProbabilityScore);
        }

        [Fact]
        public void ReviewPrediction_HasColumnNameAttribute()
        {
            System.Reflection.PropertyInfo? isPredictedOffensiveProperty = typeof(ReviewPrediction).GetProperty("IsPredictedOffensive");
            System.Reflection.PropertyInfo? offensiveProbabilityScoreProperty = typeof(ReviewPrediction).GetProperty("OffensiveProbabilityScore");

            Assert.NotNull(isPredictedOffensiveProperty);
            Assert.NotNull(offensiveProbabilityScoreProperty);

            object? isPredictedOffensiveAttribute = isPredictedOffensiveProperty.GetCustomAttributes(typeof(Microsoft.ML.Data.ColumnNameAttribute), false).FirstOrDefault();
            object? offensiveProbabilityScoreAttribute = offensiveProbabilityScoreProperty.GetCustomAttributes(typeof(Microsoft.ML.Data.ColumnNameAttribute), false).FirstOrDefault();

            Assert.NotNull(isPredictedOffensiveAttribute);
            Assert.NotNull(offensiveProbabilityScoreAttribute);
        }
    }
}