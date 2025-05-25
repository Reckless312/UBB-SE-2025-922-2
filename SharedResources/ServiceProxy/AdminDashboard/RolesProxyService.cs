using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DataAccess.Model.AdminDashboard;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;

namespace DrinkDb_Auth.ServiceProxy.AdminDashboard
{
    public class RolesProxyService : IRolesService
    {
        private const string ApiRoute = "api/roles";
        private readonly HttpClient httpClient;

        public RolesProxyService(string baseApiUrl)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            HttpResponseMessage response = await httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Role> roles = await response.Content.ReadFromJsonAsync<List<Role>>() ?? new List<Role>();
            return roles;
        }

        public async Task<Role?> GetNextRoleInHierarchyAsync(RoleType currentRoleType)
        {
            HttpResponseMessage response = await httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Role> roles = await response.Content.ReadFromJsonAsync<List<Role>>() ?? new List<Role>();

            if (currentRoleType.Equals(RoleType.Admin))
            {
                return roles.Find(role => role.RoleType == currentRoleType);
            }

            return roles.Find(role => role.RoleType > currentRoleType);
        }
    }
}