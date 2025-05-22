using DataAccess.AutoChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DrinkDb_Auth.ServiceProxy
{
    internal class AutoCheckerProxy : IAutoCheck
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string apiBaseRoute = "api/autocheck";
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


        public AutoCheckerProxy(string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.baseUrl = baseUrl;
        }

        public async Task AddOffensiveWordAsync(string newWord)
        {
            HttpResponseMessage response = await this.httpClient.PostAsync($"{this.baseUrl}/{apiBaseRoute}/add?newWord={newWord}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> AutoCheckReview(string reviewText)
        {
            HttpResponseMessage response = await this.httpClient.PostAsJsonAsync($"{this.baseUrl}/{apiBaseRoute}/review", reviewText);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task DeleteOffensiveWordAsync(string word)
        {
            HttpResponseMessage response = await this.httpClient.DeleteAsync($"{this.baseUrl}/{apiBaseRoute}/delete?word={word}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<HashSet<string>> GetOffensiveWordsList()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{apiBaseRoute}/words");
            response.EnsureSuccessStatusCode();
            HashSet<string> words = await response.Content.ReadFromJsonAsync<HashSet<string>>(jsonOptions);
            return words ?? new HashSet<string>();
        }
    }
}
