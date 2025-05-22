using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataAccess.Model.Authentication;
using IRepository;
using DataAccess.AuthProviders;
using System.Text;

namespace DrinkDb_Auth.ServiceProxy
{
    public class BasicAuthenticationProviderServiceProxy : IBasicAuthenticationProvider
    {
        private readonly HttpClient httpClient;
        private const string ApiBaseRoute = "api/auth";

        public BasicAuthenticationProviderServiceProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(new { Username = username, Password = password }),
                    Encoding.UTF8,
                    "application/json");

                HttpResponseMessage response = await this.httpClient.PostAsync($"{ApiBaseRoute}/authenticate", content);
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