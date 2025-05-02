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
            var response = await this.httpClient.DeleteAsync(ApiBaseRoute + "/" + upgradeRequestIdentifier);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            var response = await this.httpClient.GetAsync(ApiBaseRoute);
            response.EnsureSuccessStatusCode();
            List<UpgradeRequest> upgradeRequests = await response.Content.ReadFromJsonAsync<List<UpgradeRequest>>() ?? new List<UpgradeRequest>();
            return upgradeRequests;
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            var response = await this.httpClient.GetAsync(ApiBaseRoute + "/" + upgradeRequestIdentifier);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UpgradeRequest>();
        }
    }
}
