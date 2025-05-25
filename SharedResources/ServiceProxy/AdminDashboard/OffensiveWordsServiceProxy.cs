using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Model.AdminDashboard;

namespace DrinkDb_Auth.ServiceProxy.AdminDashboard
{
    public class OffensiveWordsServiceProxy : ICheckersService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/offensiveWords";

        public OffensiveWordsServiceProxy(string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<List<string>> RunAutoCheck(List<Review> reviews)
        {
            try
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{baseUrl}/{ApiBaseRoute}/check", reviews);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
            }
            catch
            {
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

        public async void RunAICheckForOneReviewAsync(Review review)
        {
            if (review?.Content == null)
            {
                return;
            }

            try
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{baseUrl}/{ApiBaseRoute}/checkOne", review);
                response.EnsureSuccessStatusCode();
            }
            catch
            {
            }
        }

        private async Task AddWord(string word)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}/{ApiBaseRoute}");
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
                HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync($"{baseUrl}/{ApiBaseRoute}/add", new OffensiveWord { Word = word });
                postResponse.EnsureSuccessStatusCode();
            }
        }

        private async Task DeleteWord(string word)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{baseUrl}/{ApiBaseRoute}/delete/{Uri.EscapeDataString(word)}");
            response.EnsureSuccessStatusCode();
        }

        private async Task<HashSet<string>> LoadOffensiveWords()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<List<OffensiveWord>>() ?? new List<OffensiveWord>();
            return new HashSet<string>(offensiveWords.Select(w => w.Word), StringComparer.OrdinalIgnoreCase);
        }
    }
}