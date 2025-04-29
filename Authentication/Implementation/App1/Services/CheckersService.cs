// <copyright file="CheckersService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using App1.AiCheck;
    using App1.AutoChecker;
    using App1.Models;
    using App1.Repositories;
    using App1.Services;
    using Microsoft.ML;
    using Newtonsoft.Json;

    public class CheckersService : ICheckersService
    {
        private static readonly string ModelPath = Path.Combine(GetProjectRoot(), "Models", "curseword_model.zip");
        private static readonly string ProjectRoot = GetProjectRoot();
        private static readonly string LogPath = Path.Combine(ProjectRoot, "Logs", "training_log.txt");
        private readonly IReviewService reviewsService;
        private readonly IAutoCheck autoCheck;

        public CheckersService(IReviewService reviewsService, IAutoCheck autoCheck)
        {
            LogToFile(ModelPath);
            this.reviewsService = reviewsService;
            this.autoCheck = autoCheck;
        }

        public List<string> RunAutoCheck(List<Review> receivedReviews)
        {
            List<string> checkingMessages = new List<string>();

            foreach (Review currentReview in receivedReviews)
            {
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
            return this.autoCheck.GetOffensiveWordsList();
        }

        public void AddOffensiveWord(string newWord)
        {
            this.autoCheck.AddOffensiveWord(newWord);
        }

        public void DeleteOffensiveWord(string word)
        {
            this.autoCheck.DeleteOffensiveWord(word);
        }

        public void RunAICheckForOneReview(Review review)
        {
            if (review == null)
            {
                Console.WriteLine("Review not found.");
                return;
            }

            bool reviewIsOffensive = CheckReviewWithAI(review.Content);
            if (!reviewIsOffensive)
            {
                Console.WriteLine("Review not found.");
                return;
            }

            Console.WriteLine($"Review {review.ReviewId} is offensive. Hiding the review.");
            this.reviewsService.HideReview(review.ReviewId);
            this.reviewsService.ResetReviewFlags(review.ReviewId);
        }

        private static void LogToFile(string message)
        {
            File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
        }

        private static string GetProjectRoot([CallerFilePath] string filePath = "")
        {
            DirectoryInfo? directory = new FileInfo(filePath).Directory;
            while (directory != null && !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new Exception("Project root not found!");
        }

        private static bool CheckReviewWithAI(string reviewText)
        {
            string analysisReportResult = OffensiveTextDetector.DetectOffensiveContent(reviewText);
            Console.WriteLine("Hugging Face Response: " + analysisReportResult);
            float offesiveContentThreshold = 0.1f;
            float hateSpeachScore = GetConfidenceScoreForHateSpeach(analysisReportResult);
            return hateSpeachScore >= offesiveContentThreshold;
        }

        private static float GetConfidenceScoreForHateSpeach(string analisysReportAsJsonString)
        {
            try
            {
                List<List<Dictionary<string, string>>>? analisysReportToList = JsonConvert.DeserializeObject<List<List<Dictionary<string, string>>>>(analisysReportAsJsonString);
                List<Dictionary<string, string>>? analisysReportToListForCurrentReview = analisysReportToList?.FirstOrDefault() ?? null;
                if (analisysReportToListForCurrentReview != null && analisysReportToListForCurrentReview.Count != 0)
                {
                    foreach (Dictionary<string, string> statisticForCharacteristic in analisysReportToListForCurrentReview)
                    {
                        if (statisticForCharacteristic.ContainsKey("label") && statisticForCharacteristic["label"] == "hate" && statisticForCharacteristic.ContainsKey("score"))
                        {
                            return float.Parse(statisticForCharacteristic["score"]);
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Deserialization error: " + ex.Message);
                return 0;
            }
        }
    }
}