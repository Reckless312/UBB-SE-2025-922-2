namespace DataAccess.Service.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.AutoChecker;
    using DataAccess.Service.AdminDashboard.Components;
    using DataAccess.Service.AdminDashboard.Interfaces;
    using Microsoft.ML;
    using Newtonsoft.Json;

    public class CheckersService : ICheckersService
    {
        private readonly IReviewService reviewsService;
        private readonly IAutoCheck autoCheck;

        public CheckersService(IReviewService reviewsService, IAutoCheck autoCheck)
        {
            this.reviewsService = reviewsService;
            this.autoCheck = autoCheck;
        }

        public List<string> RunAutoCheck(List<Review> receivedReviews)
        {
            if (receivedReviews == null)
            {
                return new List<string>();
            }

            List<string> checkingMessages = new List<string>();

            foreach (Review currentReview in receivedReviews)
            {
                if (currentReview?.Content == null)
                {
                    continue;
                }

                bool reviewIsOffensive = this.autoCheck.AutoCheckReview(currentReview.Content);
                if (reviewIsOffensive)
                {
                    checkingMessages.Add($"Review {currentReview.ReviewId} is offensive. Hiding the review.");
                    this.reviewsService.HideReview(currentReview.ReviewId);
                    this.reviewsService.ResetReviewFlags(currentReview.ReviewId);
                }
                else
                {
                    checkingMessages.Add($"Review {currentReview.ReviewId} is not offensive.");
                }
            }

            return checkingMessages;
        }

        public HashSet<string> GetOffensiveWordsList()
        {
            return this.autoCheck?.GetOffensiveWordsList() ?? new HashSet<string>();
        }

        public void AddOffensiveWord(string newWord)
        {
            if (string.IsNullOrWhiteSpace(newWord))
            {
                return;
            }

            this.autoCheck?.AddOffensiveWord(newWord);
        }

        public void DeleteOffensiveWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            this.autoCheck?.DeleteOffensiveWord(word);
        }

        public void RunAICheckForOneReview(Review review)
        {
            if (review?.Content == null)
            {
                return;
            }

            bool reviewIsOffensive = CheckReviewWithAI(review);
            if (!reviewIsOffensive)
            {
                return;
            }

            this.reviewsService.HideReview(review.ReviewId);
            this.reviewsService.ResetReviewFlags(review.ReviewId);
        }

        private static bool CheckReviewWithAI(Review review)
        {
            if (review?.Content == null)

            {
                return false;
            }

            try
            {
                string response = OffensiveTextDetector.DetectOffensiveContent(review.Content);

                if (string.IsNullOrEmpty(response) || response.StartsWith("Error:") || response.StartsWith("Exception:"))
                {
                    return false;
                }

                try
                {
                    List<List<Dictionary<string, object>>> arrayResults = JsonConvert.DeserializeObject<List<List<Dictionary<string, object>>>>(response);
                    if (arrayResults?.Count > 0 && arrayResults[0]?.Count > 0)
                    {
                        Dictionary<string, object> prediction = arrayResults[0][0];
                        if (prediction != null && 
                            prediction.TryGetValue("label", out object labelObj) &&
                            prediction.TryGetValue("score", out object scoreObj) &&
                            labelObj != null && scoreObj != null)
                        {
                            string label = labelObj.ToString().ToLower();
                            if (double.TryParse(scoreObj.ToString(), out double score))
                            {
                                if (label == "offensive" && score > 0.6)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}