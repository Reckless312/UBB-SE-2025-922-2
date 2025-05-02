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
    using DataAccess.Model.Authentication;
    using IRepository;

    public class UserProxyRepository : IUserRepository
    {
        private const string ApiRoute = "api/users";
        private HttpClient httpClient;

        public UserProxyRepository(string baseRoute) { 
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseRoute);
        }

        public async Task AddRoleToUser(Guid userID, Role roleToAdd)
        {
            var userUrl = $"{ApiRoute}/{userID}/roles";
            var response = await this.httpClient.PostAsJsonAsync(userUrl, roleToAdd);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> CreateUser(User user)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public async Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            throw new NotImplementedException();
        }

        public async Task<RoleType> GetHighestRoleTypeForUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public async Task<List<User>> GetUsersByRoleType(RoleType roleType)
        {
            throw new NotImplementedException();
        }

        public async Task<List<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ValidateAction(Guid userId, string resource, string action)
        {
            throw new NotImplementedException();
        }
    }
}
