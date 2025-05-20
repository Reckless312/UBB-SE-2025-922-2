using System.Diagnostics;
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
        private const string ApiBaseRoute = "api/offensiveWords";
        private readonly HttpClient httpClient;

        public OffensiveWordsProxyRepository(IHttpClientFactory httpClientFactory)
        {
            try
            {
                this.httpClient = httpClientFactory.CreateClient("DrinkDbClient");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddWord(string word)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync(ApiBaseRoute);
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<List<OffensiveWord>>() ?? new List<OffensiveWord>();
            OffensiveWord? searchedWord  = null;
            foreach (OffensiveWord offensive in offensiveWords)
                if (offensive.Word == word)
                    searchedWord = offensive;

            if (searchedWord == null)
            {
                HttpResponseMessage postResponse = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}/add", new OffensiveWord { Word = word });
                postResponse.EnsureSuccessStatusCode();
             }
        }

        public async Task DeleteWord(string word)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync(ApiBaseRoute);
            response.EnsureSuccessStatusCode();

            List<OffensiveWord> offensiveWords = await response.Content.ReadFromJsonAsync<List<OffensiveWord>>() ?? new List<OffensiveWord>();
            OffensiveWord? searchedWord = null;
            foreach (OffensiveWord offensive in offensiveWords)
                if (offensive.Word == word)
                    searchedWord = offensive;
            if (searchedWord != null)
            {
                HttpResponseMessage deleteResponse = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/delete/{word}");
                deleteResponse.EnsureSuccessStatusCode();
            }
        }

        public Task<HashSet<string>> LoadOffensiveWords()
        {
            try
            {

                var response = this.httpClient.GetAsync(ApiBaseRoute).Result;
                
                response.EnsureSuccessStatusCode();
                
                List<OffensiveWord> offensiveWords = response.Content.ReadFromJsonAsync<List<OffensiveWord>>().Result ?? new List<OffensiveWord>();
                
                HashSet<string> wordsAsStrings = new HashSet<string>();
                foreach (OffensiveWord word in offensiveWords)
                    wordsAsStrings.Add(word.Word);
                    
                return Task.FromResult(wordsAsStrings);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
