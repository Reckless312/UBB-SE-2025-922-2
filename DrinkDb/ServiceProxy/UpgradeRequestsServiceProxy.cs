using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using DataAccess.Model.AdminDashboard;
using DataAccess.Service.AdminDashboard.Interfaces;
using System;
using System.Text;

namespace DrinkDb_Auth.ServiceProxy
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

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UpgradeRequest>>(json) ?? new List<UpgradeRequest>();
        }

        public async Task ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(isRequestAccepted),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await this.httpClient.PostAsync(
                $"{this.baseUrl}/{ApiBaseRoute}/{upgradeRequestIdentifier}/process", 
                content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            return roleType.ToString();
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            HttpResponseMessage response = await this.httpClient.DeleteAsync($"{this.baseUrl}/{ApiBaseRoute}/{upgradeRequestIdentifier}/delete");
            response.EnsureSuccessStatusCode();
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}/{upgradeRequestIdentifier}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UpgradeRequest>(json);
        }
    }
} 