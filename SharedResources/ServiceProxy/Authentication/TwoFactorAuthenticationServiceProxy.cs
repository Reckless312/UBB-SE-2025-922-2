using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DataAccess.Model.Authentication;
using DataAccess.Service.Authentication.Interfaces;

namespace DrinkDb_Auth.ServiceProxy.Authentication
{
    public class TwoFactorAuthenticationServiceProxy : ITwoFactorAuthenticationService
    {
        private readonly HttpClient httpClient;
        private const string ApiBaseRoute = "api/2fa";

        public Guid UserId { get; set; }
        public bool IsFirstTimeSetup { get; set; }

        public TwoFactorAuthenticationServiceProxy(string baseUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<(User? currentUser, string uniformResourceIdentifier, byte[] twoFactorSecret)> Get2FAValues()
        {
            TwoFASetupRequest request = new TwoFASetupRequest { UserId = UserId, IsFirstTimeSetup = IsFirstTimeSetup };
            StringContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync($"{ApiBaseRoute}/setup", content);

            if (!response.IsSuccessStatusCode)
            {
                return (null, string.Empty, Array.Empty<byte>());
            }

            string json = await response.Content.ReadAsStringAsync();
            TwoFASetupResponse? result = JsonConvert.DeserializeObject<TwoFASetupResponse>(json);
            if (result == null || result.User == null || result.UniformResourceIdentifier == null)
            {
                return (null, string.Empty, Array.Empty<byte>());
            }
            return (result.User, result.UniformResourceIdentifier, Convert.FromBase64String(result.TwoFactorSecret ?? string.Empty));
        }

        public async Task<User?> GetUserById(Guid userId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"api/auth/user?userId={userId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(json);
        }

        public async Task<bool> UpdateUser(User user)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PutAsync($"api/auth/user/{user.UserId}", content);
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
