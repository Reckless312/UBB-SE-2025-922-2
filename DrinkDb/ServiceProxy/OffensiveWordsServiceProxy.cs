using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DataAccess.Model.AutoChecker;
using IRepository;

namespace DrinkDb.ProxyRepository.ServerProxy
{
    public class OffensiveWordsServiceProxy : IOffensiveWordsRepository
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/offensiveWords";

        public OffensiveWordsServiceProxy(HttpClient httpClient, string baseUrl)
        {
            this.httpClient = httpClient;
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task AddWord(string word)
        {
            var response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
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
                var addResponse = await this.httpClient.PostAsJsonAsync($"{this.baseUrl}/{ApiBaseRoute}/add", new OffensiveWord { Word = word });
                addResponse.EnsureSuccessStatusCode();
            }
        }

        public async Task DeleteWord(string word)
        {
            var response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
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
                var deleteResponse = await this.httpClient.DeleteAsync($"{this.baseUrl}/{ApiBaseRoute}/delete/{word}");
                deleteResponse.EnsureSuccessStatusCode();
            }
        }

        public HashSet<string> LoadOffensiveWords()
        {
            var response = this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}").Result;
            response.EnsureSuccessStatusCode();
            
            List<OffensiveWord> offensiveWords = response.Content.ReadFromJsonAsync<List<OffensiveWord>>().Result ?? new List<OffensiveWord>();
            HashSet<string> wordsAsStrings = new HashSet<string>();
            
            foreach (OffensiveWord word in offensiveWords)
            {
                wordsAsStrings.Add(word.Word);
            }
            
            return wordsAsStrings;
        }
    }
} 