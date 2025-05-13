namespace DrinkDb_Auth.ProxyRepository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using IRepository;

    public class UserProxyRepository : IUserRepository
    {
        private const string ApiRoute = "api/users";
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public UserProxyRepository()
        {

                this.httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("http://localhost:5280/");
        }
        public UserProxyRepository(string baseRoute)
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseRoute);
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task ChangeRoleToUser(Guid userId, Role roleToAdd)
        {
            var userUrl = $"{ApiRoute}/byId/{userId}/addRole";
            var response = this.httpClient.PatchAsJsonAsync(userUrl, roleToAdd).Result;
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> CreateUser(User user)
        {
            var response = this.httpClient.PostAsJsonAsync($"{ApiRoute}/add", user).Result;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<List<User>> GetAllUsers()
        {
            var response = this.httpClient.GetAsync(ApiRoute).Result;
            response.EnsureSuccessStatusCode();
            var users =  response.Content.ReadFromJsonAsync<List<User>>(jsonOptions).Result;
            return users ?? new List<User>();
        }

        public async Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            var response = this.httpClient.GetAsync($"{ApiRoute}/banned/appealed").Result;
            response.EnsureSuccessStatusCode();
            var users = response.Content.ReadFromJsonAsync<List<User>>(jsonOptions).Result;
            return users ?? new List<User>();
        }

        public async Task<RoleType> GetRoleTypeForUser(Guid userId)
        {
            var response = this.httpClient.GetAsync($"{ApiRoute}/byId/{userId}/role").Result;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RoleType>(jsonOptions);
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            var response = this.httpClient.GetAsync($"{ApiRoute}/byId/{userId}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<User>(jsonOptions).Result;
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            List<User> users = GetAllUsers().Result;
            foreach (User user in users)
            {
                if (user.Username == username)
                    return user;
            }
            return null;

            //var response = this.httpClient.GetAsync($"{ApiRoute}/byUserName/{username}").Result;
            //response.EnsureSuccessStatusCode();
            //try
            //{
            //    return await response.Content.ReadFromJsonAsync<User>(jsonOptions);
            //}
            //catch (Exception) {
            //    return null;
            //}
        }

        public async Task<List<User>> GetUsersByRoleType(RoleType roleType)
        {
            var response = await this.httpClient.GetAsync($"{ApiRoute}/byRole/{roleType}");
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<List<User>>(jsonOptions);
            return users ?? new List<User>();
        }

        public async Task<List<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            var response = await this.httpClient.GetAsync($"{ApiRoute}/appealed");
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<List<User>>(jsonOptions);
            return users ?? new List<User>();
        }

        public async Task<bool> UpdateUser(User user)
        {
            var response = this.httpClient.PatchAsJsonAsync($"{ApiRoute}/{user.UserId}/updateUser", user).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<bool>().Result;
        }

        public async Task<bool> ValidateAction(Guid userId, string resource, string action)
        {
            var response = await this.httpClient.GetAsync($"{ApiRoute}/validateAction?userID={userId}&resource={resource}&action={action}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}