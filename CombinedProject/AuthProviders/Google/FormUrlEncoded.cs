using System.Collections.Generic;
using System.Net.Http;

namespace CombinedProject.AuthProviders.Google
{
    public class FormUrlEncoded : IFormUrlEncodedContent
    {
        private readonly FormUrlEncodedContent content;

        public FormUrlEncoded(Dictionary<string, string> tokenRequest)
        {
            content = new FormUrlEncodedContent(tokenRequest);
        }
    }
}
