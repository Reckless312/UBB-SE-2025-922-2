using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Model.Authentication;
using DataAccess.OAuthProviders;
using DataAccess.Service.Authentication;
using DataAccess.Service.Authentication.Interfaces;
using Newtonsoft.Json;

namespace DrinkDb_Auth.ServiceProxy.Authentication
{
    public class AuthenticationServiceProxy : IAuthenticationService
    {
        private readonly HttpClient httpClient;
        private const string ApiBaseRoute = "api/auth";

        public AuthenticationServiceProxy(string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<AuthenticationResponse> AuthWithUserPass(string username, string password)
        {
            StringContent content = new StringContent(
                JsonConvert.SerializeObject(new { Username = username, Password = password }),
                Encoding.UTF8,
                "application/json");

            HttpResponseMessage response = await httpClient.PostAsync($"{ApiBaseRoute}/login", content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            AuthenticationResponse? authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(json);
            if (authenticationResponse == null)
            {
                return new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = null, SessionId = Guid.Empty };
            }
            return authenticationResponse;
        }

        public async Task<AuthenticationResponse> AuthWithOAuth(OAuthService selectedService, object authProvider)
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{ApiBaseRoute}/oauth?service={selectedService}", null);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
           AuthenticationResponse? authenticationResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(json);
            if (authenticationResponse == null)
            {
                return new AuthenticationResponse { AuthenticationSuccessful = false, NewAccount = false, OAuthToken = null, SessionId = Guid.Empty };
            }
            return authenticationResponse;
        }

        public async Task<User?> GetUser(Guid sessionId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"api/auth/user?sessionId={sessionId}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(json);
        }

        public async void Logout()
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{ApiBaseRoute}/logout", null);
            response.EnsureSuccessStatusCode();
        }
    }
}