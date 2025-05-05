using DataAccess.Model.AutoChecker;
using IRepository;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
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
        private const string ApiBaseRoute = "offensiveWords";
        private HttpClient httpClient;

        public OffensiveWordsProxyRepository()
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri("http://localhost:5280/");
        }

        public OffensiveWordsProxyRepository(string baseApiUrl) {
            this.httpClient  = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        public async Task AddWord(string word)
        {

            var response = this.httpClient.GetAsync(ApiBaseRoute).Result;
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = response.Content.ReadFromJsonAsync<List<OffensiveWord>>().Result ?? new List<OffensiveWord>();
            OffensiveWord? searchedWord  = null;
            foreach (OffensiveWord offensive in offensiveWords)
                if (offensive.Word == word)
                    searchedWord = offensive;

            if (searchedWord == null)
            {
                response = this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/add", new OffensiveWord { Word = word }).Result;
                response.EnsureSuccessStatusCode();
             }
        }

        public async Task DeleteWord(string word)
        {
            var response = await this.httpClient.GetAsync(ApiBaseRoute);
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = response.Content.ReadFromJsonAsync<List<OffensiveWord>>().Result ?? new List<OffensiveWord>();
            OffensiveWord? searchedWord = null;
            foreach (OffensiveWord offensive in offensiveWords)
                if (offensive.Word == word)
                    searchedWord = offensive;
            if (searchedWord != null)
            {
                response = this.httpClient.DeleteAsync($"{ApiBaseRoute}/delete/{word}").Result;
                response.EnsureSuccessStatusCode();
            }
        }

        public HashSet<string> LoadOffensiveWords()
        {
            var response = this.httpClient.GetAsync(ApiBaseRoute).Result;
            response.EnsureSuccessStatusCode();
             List < OffensiveWord > offensiveWords = response.Content.ReadFromJsonAsync<List<OffensiveWord>>().Result ?? new List<OffensiveWord>();
            HashSet<string> wordsAsStrings = new HashSet<string>();
            foreach (OffensiveWord word in offensiveWords)
                wordsAsStrings.Add(word.Word);
            return wordsAsStrings;
        }
    }
}
