using System.Net.Http;
using System.Threading.Tasks;

namespace DrinkDb_Auth.AuthProviders.Google
{
    public class ClientHttp : IHttpClient
    {
        private readonly HttpClient httpClient;

        public ClientHttp()
        {
            httpClient = new HttpClient();
        }

        public async Task<HttpResponseMessage> PostAsync(string endPoint, FormUrlEncodedContent content)
        {
            return await httpClient.PostAsync(endPoint, content);
        }
    }
}
