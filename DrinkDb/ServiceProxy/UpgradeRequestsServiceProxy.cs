using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using DataAccess.Model.AdminDashboard;
using DataAccess.Service.AdminDashboard.Interfaces;
using System;
using System.Text;

namespace DrinkDb.ServiceProxy
{
    public class UpgradeRequestsServiceProxy : IUpgradeRequestsService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/upgradeRequests";

        public UpgradeRequestsServiceProxy(HttpClient httpClient, string baseUrl)
        {
            this.httpClient = httpClient;
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public List<UpgradeRequest> RetrieveAllUpgradeRequests()
        {
            return RetrieveAllUpgradeRequestsAsync().GetAwaiter().GetResult();
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequestsAsync()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UpgradeRequest>>(json) ?? new List<UpgradeRequest>();
        }

        public void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            ProcessUpgradeRequestAsync(isRequestAccepted, upgradeRequestIdentifier).GetAwaiter().GetResult();
        }

        private async Task ProcessUpgradeRequestAsync(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(new { IsAccepted = isRequestAccepted }),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await this.httpClient.PostAsync(
                $"{this.baseUrl}/{ApiBaseRoute}/{upgradeRequestIdentifier}/process", 
                content);
            response.EnsureSuccessStatusCode();
        }

        public string GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            return roleType.ToString();
        }

        public async Task RemoveUpgradeRequestByIdentifierAsync(int upgradeRequestIdentifier)
        {
            HttpResponseMessage response = await this.httpClient.DeleteAsync($"{this.baseUrl}/{ApiBaseRoute}/{upgradeRequestIdentifier}/delete");
            response.EnsureSuccessStatusCode();
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifierAsync(int upgradeRequestIdentifier)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}/{upgradeRequestIdentifier}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UpgradeRequest>(json);
        }
    }
} 