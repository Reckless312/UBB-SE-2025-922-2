using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DrinkDb_Auth.AuthProviders.LinkedIn
{
    public class LinkedInLocalOAuthServer : ILinkedInLocalOAuthServer
    {
        private readonly HttpListener listener;
        public static event Action<string>? OnCodeReceived;
        private bool isRunning;

        public LinkedInLocalOAuthServer(string prefix)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
        }

        public async Task StartAsync()
        {
            isRunning = true;
            listener.Start();
            Console.WriteLine("LinkedIn local OAuth server listening on: " + string.Join(", ", listener.Prefixes));

            while (isRunning && listener.IsListening)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    if (context.Request.Url == null)
                    {
                        throw new Exception("Request URL is null.");
                    }
                    if (context.Request.Url.AbsolutePath.Equals("/auth", StringComparison.OrdinalIgnoreCase))
                    {
                        // LinkedIn redirects here with ?code=...
                        string code = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("code") ?? throw new Exception("No code found in the request.");

                        string responseHtml = GetHtmlResponse(code);
                        byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.ContentType = "text/html; charset=utf-8";
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    else if (context.Request.Url.AbsolutePath.Equals("/exchange", StringComparison.OrdinalIgnoreCase) &&
                             context.Request.HttpMethod == "POST")
                    {
                        using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                        {
                            string code = (await reader.ReadToEndAsync()).Trim();
                            if (!string.IsNullOrEmpty(code))
                            {
                                Console.WriteLine("LinkedIn code received: " + code);
                                OnCodeReceived?.Invoke(code);
                            }
                            else
                            {
                                Console.WriteLine("No LinkedIn code found.");
                            }
                        }
                        context.Response.StatusCode = 200;
                        context.Response.OutputStream.Close();
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        context.Response.OutputStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in LinkedInLocalOAuthServer: " + ex.Message);
                    break;
                }
            }
        }

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
                        <title>LinkedIn OAuth Login Successful!</title>
                        <script>
                            window.onload = () => {{
                                fetch('http://localhost:8891/exchange', {{
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
