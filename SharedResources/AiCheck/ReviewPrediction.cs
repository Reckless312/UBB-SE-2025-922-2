namespace DataAccess.AiCheck
{
    using Microsoft.ML.Data;

    public class ReviewPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsPredictedOffensive { get; set; }

        [ColumnName("Score")]
        public float OffensiveProbabilityScore { get; set; }
    }
}