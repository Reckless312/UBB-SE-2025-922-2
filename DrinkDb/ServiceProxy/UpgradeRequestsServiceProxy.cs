using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using DataAccess.Model.AdminDashboard;

namespace DrinkDb.ProxyRepository.ServerProxy
{
    public class UpgradeRequestsServiceProxy
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public UpgradeRequestsServiceProxy(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<List<UpgradeRequest>> GetAllUpgradeRequestsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/upgraderequests");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UpgradeRequest>>(json);
        }

        public async Task RemoveUpgradeRequestsFromBannedUsersAsync()
        {
            // Assumes a DELETE or POST endpoint exists for this operation
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/upgraderequests/remove-banned", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetRoleNameBasedOnIdentifierAsync(string roleType)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/upgraderequests/role-name/{roleType}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequestsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/upgraderequests");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UpgradeRequest>>(json);
        }

        public async Task ProcessUpgradeRequestAsync(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/upgraderequests/process?isRequestAccepted={isRequestAccepted}&upgradeRequestIdentifier={upgradeRequestIdentifier}", null);
            response.EnsureSuccessStatusCode();
        }

        public void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            // Synchronous wrapper for async method
            ProcessUpgradeRequestAsync(isRequestAccepted, upgradeRequestIdentifier).GetAwaiter().GetResult();
        }

        // Add more methods as needed, matching UpgradeRequestsService API
    }
} 