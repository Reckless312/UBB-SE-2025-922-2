namespace DataAccess.Service.AdminDashboard.Components
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public static class OffensiveTextDetector
    {
        private static readonly string HuggingFaceApiUrl = "https://api-inference.huggingface.co/models/cardiffnlp/twitter-roberta-base-offensive";
        private static readonly string HuggingFaceApiToken;
        static OffensiveTextDetector()
        {
            string projectRoot = GetProjectRoot();
            IConfiguration? configuration = new ConfigurationBuilder()
                .SetBasePath(projectRoot)
                .AddJsonFile("appsettings.json")
                .Build();

            OffensiveTextDetector.HuggingFaceApiToken = configuration["HuggingFaceApiToken"] ?? string.Empty;
        }

        public static string DetectOffensiveContent(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "Error: Empty text provided";
            }

            return TryApiRequest(OffensiveTextDetector.HuggingFaceApiUrl, text);
        }

        private static string GetProjectRoot([CallerFilePath] string filePath = "")
        {
            DirectoryInfo? directory = new FileInfo(filePath).Directory;
            while (directory != null && !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? throw new Exception("Project root not found!");
        }

        private static string TryApiRequest(string apiUrl, string text)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {HuggingFaceApiToken}");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 DrinkDBApp");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Would change from var but it's "anonymous type"
            var payload = new { inputs = text };
            StringContent jsonContent = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");

            try
            {
                HttpResponseMessage response = client.PostAsync(apiUrl, jsonContent).GetAwaiter().GetResult();
                string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    return responseContent;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return "Error: Model not found. Please check the API endpoint.";
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return "Error: Unauthorized. Please check your API token.";
                }

                return $"Error: {response.StatusCode} - {responseContent}";
            }
            catch (HttpRequestException ex)
            {
                return $"Error: Network error - {ex.Message}";
            }
            catch (TaskCanceledException)
            {
                return "Error: Request timed out. Please try again.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}