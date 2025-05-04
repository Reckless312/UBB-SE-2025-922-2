using DataAccess.Model.AutoChecker;
using IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DrinkDb_Auth.ProxyRepository.AutoChecker
{
    public class OffensiveWordsProxyRepository : IOffensiveWordsRepository
    {
        private const string ApiBaseRoute = "offenssiveWords";
        private HttpClient httpClient;

        public OffensiveWordsProxyRepository(string baseApiUrl) {
            this.httpClient  = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        public async Task AddWord(string word)
        {

            var response = await this.httpClient.GetAsync(ApiBaseRoute);
            response.EnsureSuccessStatusCode();

            HashSet<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<HashSet<OffensiveWord>>() ?? new HashSet<OffensiveWord>();

            OffensiveWord? searchedWord = offensiveWords.FirstOrDefault(currentWord => string.Equals(currentWord.Word, word, StringComparison.OrdinalIgnoreCase));

            if (searchedWord == null)
            {
                response = await this.httpClient.PostAsJsonAsync(ApiBaseRoute, word);
                response.EnsureSuccessStatusCode();
             }
        }

        public async Task DeleteWord(string word)
        {
            var response = await this.httpClient.GetAsync(ApiBaseRoute);
            response.EnsureSuccessStatusCode();

            HashSet<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<HashSet<OffensiveWord>>() ?? new HashSet<OffensiveWord>();

            OffensiveWord? searchedWord = offensiveWords.FirstOrDefault(currentWord => string.Equals(currentWord.Word, word, StringComparison.OrdinalIgnoreCase));

            if (searchedWord != null)
            {
                response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{searchedWord.OffensiveWordId}");
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<HashSet<string>> LoadOffensiveWords()
        {
            var response = await this.httpClient.GetAsync(ApiBaseRoute);
            response.EnsureSuccessStatusCode();
            HashSet<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<HashSet<OffensiveWord>>() ?? new HashSet<OffensiveWord>();
            return (HashSet<string>)(offensiveWords.Select(word => word.Word) ?? new HashSet<string>());
        }
    }
}
