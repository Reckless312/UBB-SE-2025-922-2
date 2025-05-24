using System;
using System.Net.Http;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Interfaces;
using Newtonsoft.Json;

namespace DrinkDb_Auth.ServiceProxy.Authentication
{
    public class SessionServiceProxy : ISessionService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/sessions";

        public SessionServiceProxy(string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<Session?> CreateSessionAsync(Guid userId)
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{baseUrl}/{ApiBaseRoute}/add?userId={userId}", null);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        public async Task<bool> EndSessionAsync(Guid sessionId)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{baseUrl}/{ApiBaseRoute}/{sessionId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Session?> GetSessionAsync(Guid sessionId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}/{ApiBaseRoute}/{sessionId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        public async Task<Session?> GetSessionByUserIdAsync(Guid userId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}/{ApiBaseRoute}/by-user/{userId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        public async Task<bool> AuthorizeActionAsync(Guid sessionId, string resource, string action)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{baseUrl}/{ApiBaseRoute}/{sessionId}/authorize?resource={resource}&action={action}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<bool>(json);
        }
    }
}