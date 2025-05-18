using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Interfaces;
using DataAccess.OAuthProviders;
using DataAccess.Service.Authentication;
using System.Text;

namespace DrinkDb.ServiceProxy
{
    public class AuthenticationServiceProxy : IAuthenticationService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/auth";

        public AuthenticationServiceProxy(HttpClient httpClient, string baseUrl)
        {
            this.httpClient = httpClient;
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<AuthenticationResponse> AuthWithUserPass(string username, string password)
        {
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(new { Username = username, Password = password }),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{this.baseUrl}/{ApiBaseRoute}/login", content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthenticationResponse>(json);
        }

        public async Task<AuthenticationResponse> AuthWithOAuth(OAuthService selectedService, object authProvider)
        {
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(new { Service = selectedService, Provider = authProvider }),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{this.baseUrl}/{ApiBaseRoute}/oauth", content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AuthenticationResponse>(json);
        }

        public async Task<User> GetUser(Guid sessionId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}/user/{sessionId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(json);
        }

        public async void Logout()
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{this.baseUrl}/{ApiBaseRoute}/logout", null);
            response.EnsureSuccessStatusCode();
        }
    }
} 