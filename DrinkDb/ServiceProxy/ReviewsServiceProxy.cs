using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Linq;
using System.Net.Http.Json;
using DataAccess.Model.AdminDashboard;

namespace DrinkDb_Auth.ServiceProxy
{
    public class ReviewsServiceProxy
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private const string ApiBaseRoute = "api/reviews";

        public ReviewsServiceProxy(HttpClient httpClient, string baseUrl)
        {
            this.httpClient = httpClient;
            this.baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<int> AddReview(Review review)
        {
            string json = JsonConvert.SerializeObject(review);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await httpClient.PostAsync($"{this.baseUrl}/{ApiBaseRoute}", content);
            response.EnsureSuccessStatusCode();
            
            return review.ReviewId + 1;
        }

        public async Task<List<Review>> GetAllReviews()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
        }

        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            List<Review> reviews = await GetAllReviews();
            double average = 0;
            int numberOfVisibleReviews = 0;
            
            foreach(Review review in reviews)
            {
                if (review.IsHidden == false)
                { 
                    average += review.Rating;
                    numberOfVisibleReviews++;
                }
            }
            
            return average / numberOfVisibleReviews;
        }

        public async Task<List<Review>> GetFlaggedReviews(int minFlags)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            List<Review> reviews = JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
            
            return reviews.Where(review => review.NumberOfFlags >= minFlags && !review.IsHidden).ToList();
        }

        public async Task<List<Review>> GetHiddenReviews()
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            List<Review> reviews = JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
            
            return reviews.Where(review => review.IsHidden).ToList();
        }

        public async Task<List<Review>> GetMostRecentReviews(int count)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            List<Review> reviews = JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
            
            return reviews.Where(review => !review.IsHidden)
                          .OrderByDescending(review => review.CreatedDate)
                          .Take(count)
                          .ToList();
        }

        public async Task<Review> GetReviewById(int reviewID)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}/{reviewID}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Review>(json);
        }

        public async Task<int> GetReviewCountAfterDate(DateTime date)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            List<Review> reviews = JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
            
            return reviews.Where(review => review.CreatedDate == date).Count();
        }

        public async Task<List<Review>> GetReviewsByUser(Guid userId)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}/by-user/{userId}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
        }

        public async Task<List<Review>> GetReviewsSince(DateTime date)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{this.baseUrl}/{ApiBaseRoute}");
            response.EnsureSuccessStatusCode();
            
            string json = await response.Content.ReadAsStringAsync();
            List<Review> reviews = JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
            
            return reviews.Where(review => review.CreatedDate >= date).ToList();
        }

        public async Task RemoveReviewById(int reviewID)
        {
            HttpResponseMessage response = await httpClient.DeleteAsync($"{this.baseUrl}/{ApiBaseRoute}/{reviewID}");
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateNumberOfFlagsForReview(int reviewID, int numberOfFlags)
        {
            string json = JsonConvert.SerializeObject(numberOfFlags);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await httpClient.PatchAsync($"{this.baseUrl}/{ApiBaseRoute}/{reviewID}/updateFlags", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateReviewVisibility(int reviewID, bool isHidden)
        {
            string json = JsonConvert.SerializeObject(isHidden);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await httpClient.PatchAsync($"{this.baseUrl}/{ApiBaseRoute}/{reviewID}/updateVisibility", content);
            response.EnsureSuccessStatusCode();
        }
    }
} 