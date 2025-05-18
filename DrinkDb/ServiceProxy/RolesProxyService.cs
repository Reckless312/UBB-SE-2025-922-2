using DataAccess.Model.AdminDashboard;
using IRepository;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DrinkDb_Auth.ServiceProxy
{
    public class RolesProxyService : IRolesRepository
    {
        private const string ApiRoute = "api/roles";
        private readonly HttpClient httpClient;

        public RolesProxyService(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        public async Task<List<Role>> GetAllRoles()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Role> roles = await response.Content.ReadFromJsonAsync<List<Role>>() ?? new List<Role>();
            return roles;
        }

        public async Task<Role> GetNextRoleInHierarchy(RoleType currentRoleType)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Role> roles = await response.Content.ReadFromJsonAsync<List<Role>>() ?? new List<Role>();

            if (currentRoleType.Equals(RoleType.Manager))
            {
                return roles.Find(role => role.RoleType == currentRoleType);
            }

            return roles.Find(role => role.RoleType > currentRoleType);
        }
    }
} 