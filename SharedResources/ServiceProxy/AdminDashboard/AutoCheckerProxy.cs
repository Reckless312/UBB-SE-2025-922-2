using System.Net.Http.Json;
using System.Text.Json;
using DataAccess.AutoChecker;

namespace DrinkDb_Auth.ServiceProxy.AdminDashboard
{
    public class AutoCheckerProxy : IAutoCheck
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string API_BASE_ROUTE = "api/autocheck";
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public AutoCheckerProxy(string baseUrl)
        {
            httpClient = new HttpClient();
            this.baseUrl = baseUrl;
        }

        public async Task AddOffensiveWordAsync(string newWord)
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{baseUrl}/{API_BASE_ROUTE}/add?newWord={newWord}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> AutoCheckReview(string reviewText)
        {
            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{baseUrl}/{API_BASE_ROUTE}/review", reviewText);
            // response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task DeleteOffensiveWordAsync(string word)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{baseUrl}/{API_BASE_ROUTE}/delete?word={word}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<HashSet<string>> GetOffensiveWordsList()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}/{API_BASE_ROUTE}/words");
            response.EnsureSuccessStatusCode();
            HashSet<string>? words = await response.Content.ReadFromJsonAsync<HashSet<string>>(jsonOptions);
            return words ?? new HashSet<string>();
        }
    }
}
