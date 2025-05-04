using DataAccess.Model.AdminDashboard;
using IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DrinkDb_Auth.ProxyRepository.AdminDashboard
{
    public class RolesProxyRepository : IRolesRepository
    {
        private const string ApiRoute = "roles";
        private HttpClient httpClient;

        public RolesProxyRepository(string baseApiUrl) {
            this.httpClient = new HttpClient(); 
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        public async Task<List<Role>> GetAllRoles()
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Role> roles = await response.Content.ReadFromJsonAsync<List<Role>>() ?? new List<Role>();
            return roles;
        }

        public async Task<Role> GetNextRoleInHierarchy(RoleType currentRoleType)
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Role> roles = await response.Content.ReadFromJsonAsync<List<Role>>() ?? new List<Role>();

            if (currentRoleType.Equals(RoleType.Manager))
            {
                return roles.Where(role => role.RoleType == currentRoleType).First();
            }

            return roles.Where(role => role.RoleType < currentRoleType).First();
        }
    }
}
