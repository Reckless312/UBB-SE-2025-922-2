using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataAccess.Model.Authentication;
using System;

namespace DrinkDb.ProxyRepository.ServerProxy
{
    public class SessionServiceProxy
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/sessions";

        public SessionServiceProxy(HttpClient httpClient, string baseUrl)
        {
            this.httpClient = httpClient;
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<Session> CreateSessionAsync(Guid userId)
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{this.baseUrl}/{ApiBaseRoute}/add?userId={userId}", null);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        public async Task<bool> EndSessionAsync(Guid sessionId)
        {
            HttpResponseMessage response = await this.httpClient.DeleteAsync($"{this.baseUrl}/{ApiBaseRoute}/{sessionId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Session> GetSessionAsync(Guid sessionId)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}/{sessionId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        public async Task<Session> GetSessionByUserIdAsync(Guid userId)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}/by-user/{userId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }
    }
} 