// <copyright file="OffensiveTextDetector.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using App1.AiCheck;
    using App1.AutoChecker;
    using App1.Models;
    using App1.Repositories;
    using App1.Services;
    using Microsoft.ML;
    using Newtonsoft.Json;

    /// <summary>
    /// class for detecting offensive content.
    /// </summary>
    public static class OffensiveTextDetector
    {
        private static readonly string HuggingFaceApiUrl = "https://api-inference.huggingface.co/models/facebook/roberta-hate-speech-dynabench-r1-target";
        private static readonly string HuggingFaceApiToken = string.Empty;

        /// <summary>
        /// Search for offensive content.
        /// </summary>
        /// <param name="text">review.</p aram>
        /// <returns>json string with the results.</returns>
        public static string DetectOffensiveContent(string text)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {HuggingFaceApiToken}");
            StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(text), Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = client.PostAsync(HuggingFaceApiUrl, jsonContent).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                return $"Error: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
    }
}