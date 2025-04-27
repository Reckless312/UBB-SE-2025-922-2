namespace UnitTests.ReviewChecker.AuxiliaryTestsClasses
{
    public static class TestHelpers
    {
        public static string MockDetectOffensiveContentHighScore(string text)
        {
            return @"[[{""label"":""hate"",""score"":""0.9""},{""label"":""not_hate"",""score"":""0.1""}]]";
        }

        public static string MockDetectOffensiveContentLowScore(string text)
        {
            return @"[[{""label"":""hate"",""score"":""0.05""},{""label"":""not_hate"",""score"":""0.95""}]]";
        }

        public static bool MockCheckReviewWithAI_True(string text)
        {
            return true;
        }

        public static bool MockCheckReviewWithAI_False(string text)
        {
            return false;
        }
    }
}
