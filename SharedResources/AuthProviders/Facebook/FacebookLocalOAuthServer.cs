using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace DataAccess.AuthProviders.Facebook
{
    public class FacebookLocalOAuthServer : IFacebookLocalOAuthServer
    {
        private HttpListener listener;

        public static event Action<string>? OnTokenReceived;

        public FacebookLocalOAuthServer(string prefix)
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(prefix);
        }

        public async Task StartAsync()
        {
            this.listener.Start();
            Console.WriteLine("Serverul local ascultă la: " + string.Join(", ", this.listener.Prefixes));

            while (this.listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await this.listener.GetContextAsync();

                    if (context.Request.Url?.AbsolutePath.Equals("/auth", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        string responseHtml = GetHtmlResponse();
                        byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);

                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.ContentType = "text/html; charset=utf-8";

                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    else if (context.Request.Url?.AbsolutePath.Equals("/token", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                        {
                            string queryParameters = await reader.ReadToEndAsync();
                            if (queryParameters.StartsWith("#"))
                            {
                                queryParameters = queryParameters.Substring(1);
                            }

                            NameValueCollection splitParameters = HttpUtility.ParseQueryString(queryParameters.Trim());

                            string accessToken = splitParameters["access_token"] ?? throw new Exception("Acess token not found.");
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                Console.WriteLine("Acess token: " + accessToken);
                                OnTokenReceived?.Invoke(accessToken);
                            }
                            else
                            {
                                Console.WriteLine("Acess token not found.");
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
                catch (Exception exception)
                {
                    Console.WriteLine("Eroare: " + exception.Message);
                    break;
                }
            }
        }

        public void Stop()
        {
            this.listener.Stop();
        }

        private string GetHtmlResponse()
        {
            return @"
            <!DOCTYPE html>
            <html>
              <head>
                <title>OAuth Log in successful!</title>
                <script>
                    window.onload = () => {
                        console.log('HI!');
                        fetch('http://localhost:8888/token', {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'text/plain'
                            },
                            body: window.location.hash
                        });
                    }
                 </script>
              </head>
              <body>
                <h1>Autentificare cu succes!</h1>
                <p id='message'>Poti inchide aceasta pagina.</p>
              </body>
            </html>";
        }
    }
}
