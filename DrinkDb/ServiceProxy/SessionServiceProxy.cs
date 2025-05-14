using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using DataAccess.Model.Authentication; // Use the actual Session type
using System;

namespace DrinkDb.ProxyRepository.ServerProxy
{
    public class SessionServiceProxy
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SessionServiceProxy(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<List<Session>> GetAllSessionsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/session");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Session>>(json);
        }

        public async Task<Session> CreateSessionAsync(Guid userId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/session/add?userId={userId}", null);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        public async Task<bool> EndSessionAsync(Guid sessionId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/session/{sessionId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Session> GetSessionAsync(Guid sessionId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/session/{sessionId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        public async Task<bool> ValidateSessionAsync(Guid sessionId)
        {
            // This assumes an endpoint exists for validation
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/session/validate/{sessionId}");
            if (!response.IsSuccessStatusCode)
                return false;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<bool>(json);
        }

        public async Task<bool> AuthorizeActionAsync(Guid sessionId, string resource, string action)
        {
            // This assumes an endpoint exists for authorization
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/session/authorize?sessionId={sessionId}&resource={resource}&action={action}");
            if (!response.IsSuccessStatusCode)
                return false;
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<bool>(json);
        }

        public async Task<Session> GetSessionByUserIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/session/by-user/{userId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Session>(json);
        }

        // Add more methods as needed, matching SessionService API
    }
} 