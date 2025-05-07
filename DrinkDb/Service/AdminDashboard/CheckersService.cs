using DrinkDb_Auth.Service.AdminDashboard.Components;
using System;

namespace DrinkDb_Auth.Service.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using DrinkDb_Auth.AutoChecker;
    using DataAccess.Model.AdminDashboard;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
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
            List<string> checkingMessages = new List<string>();

            foreach (Review currentReview in receivedReviews)
            {
                bool reviewIsOffensive = autoCheck.AutoCheckReview(currentReview.Content);
                if (reviewIsOffensive)
                {
                    checkingMessages.Add($"Review {currentReview.ReviewId} is offensive. Hiding the review.");
                    reviewsService.HideReview(currentReview.ReviewId);
                    reviewsService.ResetReviewFlags(currentReview.ReviewId);
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
            return autoCheck.GetOffensiveWordsList();
        }

        public void AddOffensiveWord(string newWord)
        {
            autoCheck.AddOffensiveWord(newWord);
        }

        public void DeleteOffensiveWord(string word)
        {
            autoCheck.DeleteOffensiveWord(word);
        }

        public void RunAICheckForOneReview(Review review)
        {
            if (review == null)
            {
                Console.WriteLine("Review not found.");
                return;
            }

            bool reviewIsOffensive = CheckReviewWithAI(review).GetAwaiter().GetResult();
            if (!reviewIsOffensive)
            {
                return;
            }

            Console.WriteLine($"Review {review.ReviewId} is offensive. Hiding the review.");
            reviewsService.HideReview(review.ReviewId);
            reviewsService.ResetReviewFlags(review.ReviewId);
        }

        private async Task<bool> CheckReviewWithAI(Review review)
        {
            try
            {
                string response = OffensiveTextDetector.DetectOffensiveContent(review.Content);

                if (response.StartsWith("Error:") || response.StartsWith("Exception:"))
                {
                    return false;
                }

                try
                {
                    var arrayResults = JsonConvert.DeserializeObject<List<List<Dictionary<string, object>>>>(response);
                    if (arrayResults != null && arrayResults.Count > 0 && arrayResults[0].Count > 0)
                    {
                        var prediction = arrayResults[0][0];
                        if (prediction.TryGetValue("label", out object labelObj) &&
                            prediction.TryGetValue("score", out object scoreObj))
                        {
                            string label = labelObj.ToString().ToLower();
                            double score = Convert.ToDouble(scoreObj);

                            if (label == "offensive" && score > 0.6)
                            {
                                return true;
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