namespace DrinkDb_Auth.ProxyRepository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;

    public class UpgradeRequestProxyRepository : IUpgradeRequestsRepository
    {
        private const string ApiBaseRoute = "api/upgradeRequests";
        private HttpClient httpClient;

        public UpgradeRequestProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            var response = this.httpClient.DeleteAsync(ApiBaseRoute + "/" + upgradeRequestIdentifier + "/delete").Result;
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            var response = this.httpClient.GetAsync(ApiBaseRoute).Result;
            response.EnsureSuccessStatusCode();
            List<UpgradeRequest> upgradeRequests = response.Content.ReadFromJsonAsync<List<UpgradeRequest>>().Result ?? new List<UpgradeRequest>();
            return upgradeRequests;
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            var response = this.httpClient.GetAsync(ApiBaseRoute + "/" + upgradeRequestIdentifier).Result;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UpgradeRequest>();
        }
    }
}
