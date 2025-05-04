using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DrinkDb_Auth.AuthProviders.Facebook
{
    public class FacebookLocalOAuthServer : IFacebookLocalOAuthServer
    {
        private readonly HttpListener listener;

        private const int HTTP_STATUS_OK = 200;
        private const int HTTP_STATUS_NOT_FOUND = 404;
        private const char FRAGMENT_PREFIX_CHARACTER = '#';
        private const int FRAGMENT_PREFIX_LENGTH = 1;

        public static event Action<string>? OnTokenReceived;

        public FacebookLocalOAuthServer(string prefix)
        {
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
        }

        public async Task StartAsync()
        {
            listener.Start();
            Console.WriteLine("Serverul local ascultă la: " + string.Join(", ", listener.Prefixes));

            while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();

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
                        using (StreamReader reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                        {
                            string queryParameters = await reader.ReadToEndAsync();
                            if (queryParameters.StartsWith(FRAGMENT_PREFIX_CHARACTER))
                            {
                                queryParameters = queryParameters.Substring(FRAGMENT_PREFIX_LENGTH);
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
                    Console.WriteLine("Error: " + exception.Message);
                    break;
                }
            }
        }

        public void Stop()
        {
            listener.Stop();
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
