using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using DataAccess.Model.AutoChecker;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Model.AdminDashboard;

namespace DrinkDb_Auth.ServerProxy
{
    public class OffensiveWordsServiceProxy : ICheckersService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/offensiveWords";

        public OffensiveWordsServiceProxy(HttpClient httpClient, string baseUrl)
        {
            this.httpClient = httpClient;
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public List<string> RunAutoCheck(List<Review> reviews)
        {
            List<string> checkingMessages = new List<string>();
            foreach (Review currentReview in reviews)
            {
                bool reviewIsOffensive = AutoCheckReview(currentReview.Content);
                if (reviewIsOffensive)
                {
                    checkingMessages.Add($"Review {currentReview.ReviewId} is offensive. Hiding the review.");
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
            return LoadOffensiveWords().Result;
        }

        public void AddOffensiveWord(string newWord)
        {
            if (string.IsNullOrWhiteSpace(newWord))
            {
                return;
            }
            AddWord(newWord).Wait();
        }

        public void DeleteOffensiveWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }
            DeleteWord(word).Wait();
        }

        public void RunAICheckForOneReview(Review review)
        {
            if (review?.Content == null)
            {
                return;
            }

            bool reviewIsOffensive = AutoCheckReview(review.Content);
            if (reviewIsOffensive)
            {
                // Note: This would need to be handled by the calling service
                // as we don't have direct access to the review service here
            }
        }

        private bool AutoCheckReview(string reviewText)
        {
            if (string.IsNullOrWhiteSpace(reviewText))
            {
                return false;
            }

            HashSet<string> offensiveWords = GetOffensiveWordsList();
            string[] words = reviewText.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Any(word => offensiveWords.Contains(word, StringComparer.OrdinalIgnoreCase));
        }

        private async Task AddWord(string word)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<List<OffensiveWord>>() ?? new List<OffensiveWord>();
            
            bool wordExists = false;
            foreach (OffensiveWord offensive in offensiveWords)
            {
                if (offensive.Word == word)
                {
                    wordExists = true;
                    break;
                }
            }

            if (!wordExists)
            {
                HttpResponseMessage addResponse = await this.httpClient.PostAsJsonAsync($"{this.baseUrl}/{ApiBaseRoute}/add", new OffensiveWord { Word = word });
                addResponse.EnsureSuccessStatusCode();
            }
        }

        private async Task DeleteWord(string word)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<List<OffensiveWord>>() ?? new List<OffensiveWord>();
            
            bool wordExists = false;
            foreach (OffensiveWord offensive in offensiveWords)
            {
                if (offensive.Word == word)
                {
                    wordExists = true;
                    break;
                }
            }

            if (wordExists)
            {
                HttpResponseMessage deleteResponse = await this.httpClient.DeleteAsync($"{this.baseUrl}/{ApiBaseRoute}/delete/{word}");
                deleteResponse.EnsureSuccessStatusCode();
            }
        }

        private async Task<HashSet<string>> LoadOffensiveWords()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            
            List<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<List<OffensiveWord>>() ?? new List<OffensiveWord>();
            HashSet<string> wordsAsStrings = new HashSet<string>();
            
            foreach (OffensiveWord word in offensiveWords)
            {
                wordsAsStrings.Add(word.Word);
            }
            
            return wordsAsStrings;
        }
    }
} 