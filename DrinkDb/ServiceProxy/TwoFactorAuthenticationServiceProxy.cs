using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Interfaces;

namespace DrinkDb_Auth.ServiceProxy
{
    public class TwoFactorAuthenticationServiceProxy : ITwoFactorAuthenticationService
    {
        private readonly HttpClient httpClient;
        private const string ApiBaseRoute = "api/2fa";

        public Guid UserId { get; set; }
        public bool IsFirstTimeSetup { get; set; }

        public TwoFactorAuthenticationServiceProxy(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public (User currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret) Get2FAValues()
        {
            TwoFASetupRequest request = new TwoFASetupRequest { UserId = this.UserId, IsFirstTimeSetup = this.IsFirstTimeSetup };
            StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = this.httpClient.PostAsync($"{ApiBaseRoute}/setup", content).GetAwaiter().GetResult();
            
            if (!response.IsSuccessStatusCode)
                throw new ArgumentException("Failed to setup 2FA for user.");
            
            string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            TwoFASetupResponse result = JsonConvert.DeserializeObject<TwoFASetupResponse>(json);
            if (result == null || result.User == null)
                throw new ArgumentException("Invalid 2FA setup response from server.");
            return (result.User, result.UniformResourceIdentifier, Convert.FromBase64String(result.TwoFactorSecret ?? string.Empty));
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            HttpResponseMessage response = await this.httpClient.GetAsync($"api/auth/user?userId={userId}");
            if (!response.IsSuccessStatusCode)
                return null;
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(json);
        }

        public async Task<bool> UpdateUser(User user)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await this.httpClient.PutAsync($"api/auth/user/{user.UserId}", content);
            return response.IsSuccessStatusCode;
        }

        private class TwoFASetupRequest
        {
            public Guid UserId { get; set; }
            public bool IsFirstTimeSetup { get; set; }
        }

        private class TwoFASetupResponse
        {
            public User? User { get; set; }
            public string? UniformResourceIdentifier { get; set; }
            public string? TwoFactorSecret { get; set; }
        }
    }
}
