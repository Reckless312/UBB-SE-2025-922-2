namespace DrinkDb_Auth.ProxyRepository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;

    public class ReviewsProxyRepository : IReviewsRepository
    {
        private const string ApiRoute = "reviews";
        private readonly HttpClient httpClient;

        public ReviewsProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        public async Task<int> AddReview(Review review)
        {
            var response = await httpClient.PostAsJsonAsync(ApiRoute, review);
            response.EnsureSuccessStatusCode();
            return review.ReviewId + 1;
        }

        public async Task<List<Review>> GetAllReviews()
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>();
            return reviews ?? new List<Review>();

        }

        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Review>> GetFlaggedReviews(int minFlags)
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();

            return reviews.Where(review => review.NumberOfFlags >= minFlags && !review.IsHidden).ToList() ?? new List<Review>();
        }

        public async Task<List<Review>> GetHiddenReviews()
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();
            return reviews.Where(review => review.IsHidden).ToList() ?? new List<Review>();
        }

        public async Task<List<Review>> GetMostRecentReviews(int count)
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();

            return reviews.Where(review => !review.IsHidden).OrderByDescending(review => review.CreatedDate).Take(count).ToList();
        }

        public async Task<Review> GetReviewById(int reviewID)
        {

            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>()?? new List<Review>();

            return reviews.Where(review => review.ReviewId == reviewID).First();
        }

        public async Task<int> GetReviewCountAfterDate(DateTime date)
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();

            return reviews.Where(review => review.CreatedDate == date).Count();
        }

        public async Task<List<Review>> GetReviewsByUser(Guid userId)
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();

            return reviews.Where(review => review.UserId == userId).ToList();
        }

        public async Task<List<Review>> GetReviewsSince(DateTime date)
        {
            var response = await this.httpClient.GetAsync(ApiRoute);
            response.EnsureSuccessStatusCode();
            List<Review> reviews = await response.Content.ReadFromJsonAsync<List<Review>>() ?? new List<Review>();

            return reviews.Where(review => review.CreatedDate >= date).ToList();
        }

        public async Task<bool> RemoveReviewById(int reviewID)
        {
            var reviewUrl = $"{ApiRoute}/{reviewID}";
            var response = await this.httpClient.DeleteAsync(reviewUrl);
            return response.IsSuccessStatusCode;
        }

        public async Task UpdateNumberOfFlagsForReview(int reviewID, int numberOfFlags)
        {
            var reviewUrl = $"{ApiRoute}/{reviewID}/flags";
            var updateData = new { numberOfFlags };
            var response = await httpClient.PutAsJsonAsync(reviewUrl, updateData);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateReviewVisibility(int reviewID, bool isHidden)
        {
            var reviewUrl = $"{ApiRoute}/{reviewID}/visibility";

            var updateData = new { isHidden };

            var response = await httpClient.PutAsJsonAsync(reviewUrl, updateData);

            response.EnsureSuccessStatusCode();
        }
    }
}
