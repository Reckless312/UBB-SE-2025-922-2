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

        public async Task<List<string>> RunAutoCheck(List<Review> reviews)
        {
            try
            {
                HttpResponseMessage response = await this.httpClient.PostAsJsonAsync($"{this.baseUrl}/{ApiBaseRoute}/check", reviews);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RunAutoCheck: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<HashSet<string>> GetOffensiveWordsList()
        {
            return await LoadOffensiveWords();
        }

        public async Task AddOffensiveWordAsync(string newWord)
        {
            if (string.IsNullOrWhiteSpace(newWord))
            {
                return;
            }
            await AddWord(newWord);
        }

        public async Task DeleteOffensiveWordAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }
            await DeleteWord(word);
        }

        public async Task RunAICheckForOneReviewAsync(Review review)
        {
            if (review?.Content == null)
            {
                return;
            }

            try
            {
                HttpResponseMessage response = await this.httpClient.PostAsJsonAsync($"{this.baseUrl}/{ApiBaseRoute}/checkOne", review);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RunAICheckForOneReviewAsync: {ex.Message}");
            }
        }

        private async Task<bool> AutoCheckReview(string reviewText)
        {
            if (string.IsNullOrWhiteSpace(reviewText))
            {
                return false;
            }

            HashSet<string> offensiveWords = await GetOffensiveWordsList();
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
                HttpResponseMessage postResponse = await this.httpClient.PostAsJsonAsync($"{this.baseUrl}/{ApiBaseRoute}/add", new OffensiveWord { Word = word });
                postResponse.EnsureSuccessStatusCode();
            }
        }

        private async Task DeleteWord(string word)
        {
            HttpResponseMessage response = await this.httpClient.DeleteAsync($"{this.baseUrl}/{ApiBaseRoute}/delete/{Uri.EscapeDataString(word)}");
            response.EnsureSuccessStatusCode();
        }

        private async Task<HashSet<string>> LoadOffensiveWords()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<List<OffensiveWord>>() ?? new List<OffensiveWord>();
            return new HashSet<string>(offensiveWords.Select(w => w.Word), StringComparer.OrdinalIgnoreCase);
        }
    }
} 