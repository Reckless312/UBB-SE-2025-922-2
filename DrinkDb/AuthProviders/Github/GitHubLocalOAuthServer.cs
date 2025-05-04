using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DrinkDb_Auth.AuthProviders.Github
{
    public class GitHubLocalOAuthServer : IGitHubLocalOAuthServer
    {
        private readonly IGitHubHttpHelper listener;

        // Status code constants
        private const int HTTP_STATUS_OK = 200;
        private const int HTTP_STATUS_BAD_REQUEST = 400;
        private const int HTTP_STATUS_NOT_FOUND = 404;

        // HTTP paths
        private const string AuthenticationPath = "/auth";
        private const string ExchangePath = "/exchange";

        // HTTP method
        private const string PostMethod = "POST";

        /// <summary>
        /// Fires when the GitHub code is received from the redirect.
        /// </summary>
        public static event Action<string>? OnCodeReceived;

        private bool isRunning = false;

        public GitHubLocalOAuthServer(string prefix)
        {
            listener = new GitHubHttpHelper();
            listener.Prefixes.Add(prefix);
        }

        public GitHubLocalOAuthServer(IGitHubHttpHelper listener)
        {
            this.listener = listener;
        }

        public async Task StartAsync()
        {

            isRunning = true;
            listener.Start();
            Console.WriteLine("GitHub local OAuth server listening on: " + string.Join(", ", listener.Prefixes));

            while (isRunning && listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    if (context.Request.Url == null)
                    {
                        context.Response.StatusCode = HTTP_STATUS_BAD_REQUEST;
                        context.Response.OutputStream.Close();
                        continue;
                    }
                    if (context.Request.Url.AbsolutePath.Equals(AuthenticationPath, StringComparison.OrdinalIgnoreCase))
                    {
                        // GitHub redirects here with ?code=...
                        // We'll show a minimal HTML that triggers a POST to /exchange with the code
                        string code = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("code") ?? throw new Exception("Code not found in request.");

                        string responseHtml = GetHtmlResponse(code);
                        byte[] responseBuffer = Encoding.UTF8.GetBytes(responseHtml);
                        context.Response.ContentLength64 = responseBuffer.Length;
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.OutputStream.WriteAsync(responseBuffer, 0, responseBuffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    else if (context.Request.Url.AbsolutePath.Equals(ExchangePath, StringComparison.OrdinalIgnoreCase)
                             && context.Request.HttpMethod == PostMethod)
                    {
                        // We read the 'code' from the request body and notify any subscribers
                        using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                        {
                            string code = (await reader.ReadToEndAsync()).Trim();
                            if (!string.IsNullOrEmpty(code))
                            {
                                Console.WriteLine("GitHub code received: " + code);
                                OnCodeReceived?.Invoke(code);
                            }
                            else
                            {
                                Console.WriteLine("No GitHub code found.");
                            }
                        }
                        context.Response.StatusCode = HTTP_STATUS_OK;
                        context.Response.OutputStream.Close();
                    }
                    else
                    {
                        context.Response.StatusCode = HTTP_STATUS_NOT_FOUND;
                        context.Response.OutputStream.Close();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error in GitHubLocalOAuthServer: " + exception.Message);
                    break;
                }
            }
        }

        // TODO delete function since it has 0 references
        public void Stop()
        {
            isRunning = false;
            listener.Stop();
        }

        private string GetHtmlResponse(string code)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                  <head>
                    <title>GitHub OAuth Login Successful!</title>
                    <script>
                        window.onload = () => {{
                            fetch('http://localhost:8890/exchange', {{
                                method: 'POST',
                                headers: {{
                                    'Content-Type': 'text/plain'
                                }},
                                body: '{code}'
                            }});
                        }}
                     </script>
                  </head>
                  <body>
                    <h1>Authentication successful!</h1>
                    <p>You can close this tab.</p>
                  </body>
                </html>";
        }
    }
}
