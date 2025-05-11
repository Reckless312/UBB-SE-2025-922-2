namespace DrinkDb_Auth.ProxyRepository.Authentification
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using DataAccess.Model.Authentication;
    using IRepository;

    public class SessionProxyRepository : ISessionRepository
    {
        private const string ApiBaseRoute = "api/sessions";
        private readonly HttpClient httpClient;

        public SessionProxyRepository()
        {
            this.httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:5280/");

        }

        public SessionProxyRepository(string baseApiUrl)
        { 
            this.httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseApiUrl)
            };
        }
        public async Task<Session> CreateSession(Guid userId)
        {
            var response = this.httpClient.PostAsync($"{ApiBaseRoute}/add?userId={userId}", null).Result;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Session>();
        }

        public async Task<bool> EndSession(Guid sessionId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{sessionId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Session> GetSession(Guid sessionId)
        {
            var response = this.httpClient.GetAsync($"{ApiBaseRoute}/{sessionId}").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<Session>().Result;
        }

        public async Task<Session> GetSessionByUserId(Guid userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/by-user/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Session>();
        }
    }
}
