using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DataAccess.AuthProviders;
using Newtonsoft.Json;

namespace DrinkDb_Auth.ServiceProxy.Authentication
{
    public class BasicAuthenticationProviderServiceProxy : IBasicAuthenticationProvider
    {
        private readonly HttpClient httpClient;
        private const string ApiBaseRoute = "api/auth";

        public BasicAuthenticationProviderServiceProxy(string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(new { Username = username, Password = password }),
                    Encoding.UTF8,
                    "application/json");

                HttpResponseMessage response = await httpClient.PostAsync($"{ApiBaseRoute}/authenticate", content);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during authentication: {ex.Message}");
                return false;
            }
        }

        public bool Authenticate(string username, string password)
        {
            return AuthenticateAsync(username, password).GetAwaiter().GetResult();
        }
    }
}