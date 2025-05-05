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

        public HashSet<string> LoadOffensiveWords()
        {
            var response = this.httpClient.GetAsync(ApiBaseRoute).Result;
            response.EnsureSuccessStatusCode();
            HashSet<OffensiveWord> offensiveWords = response.Content.ReadFromJsonAsync<HashSet<OffensiveWord>>().Result ?? new HashSet<OffensiveWord>();
            HashSet<string> wordsAsStrings = new HashSet<string>();
            foreach (OffensiveWord word in offensiveWords)
                wordsAsStrings.Add(word.Word);
            return wordsAsStrings;
        }
    }
}
