using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            this.reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
        }

        [HttpGet("")]
        public async Task<IEnumerable<Review>> GetAll()
        {
            return await this.reviewService.GetAllReviews();
        }

        [HttpGet("since")]
        public async Task<IEnumerable<Review>> GetReviewsSince([FromQuery] DateTime date)
        {
            return await this.reviewService.GetReviewsSince(date);
        }
        
        [HttpGet("averageRatingVisibleReviews")]
        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            return await this.reviewService.GetAverageRatingForVisibleReviews();
        }
        
        [HttpGet("mostRecent")]
        public async Task<IEnumerable<Review>> GetMostRecentReviews([FromQuery] int count)
        {
            return await this.reviewService.GetMostRecentReviews(count);
        }
        
        [HttpGet("countAfterDate")]
        public async Task<int> GetReviewCountAfterDate([FromQuery] DateTime date)
        {
            return await this.reviewService.GetReviewCountAfterDate(date);
        }

        [HttpGet("flagged")]
        public async Task<IEnumerable<Review>> GetFlaggedReviews([FromQuery] int minFlags)
        {
            return await this.reviewService.GetFlaggedReviews(minFlags);
        }

        [HttpGet("byUser")]
        public async Task<IEnumerable<Review>> GetReviewsByUser([FromQuery] Guid userId)
        {
            return await this.reviewService.GetReviewsByUser(userId);
        }
        
        [HttpGet("{id}")]
        public async Task<Review> GetReviewById(int id)
        {
            return await this.reviewService.GetReviewById(id);
        }

        [HttpPatch("{id}/updateFlags")]
        public async Task UpdateNumberOfFlagsForReview(int id, [FromBody] int numberOfFlags)
        {
            await this.reviewService.UpdateNumberOfFlagsForReview(id, numberOfFlags);
        }

        [HttpPatch("{id}/updateVisibility")]
        public async Task UpdateReviewVisibility(int id, [FromBody] bool isHidden)
        {
            await this.reviewService.UpdateReviewVisibility(id, isHidden);
        }

        [HttpPost("add")]
        public async Task<int> AddReview([FromBody] Review review)
        {
            return await this.reviewService.AddReview(review);
        }

        [HttpDelete("{id}/delete")]
        public async Task RemoveReviewById(int id)
        {
            await this.reviewService.RemoveReviewById(id);
        }

        [HttpGet("hidden")]
        public async Task<IEnumerable<Review>> GetHiddenReviews()
        {
            return await reviewService.GetHiddenReviews();
        }

        [HttpGet("report")]
        public async Task<IEnumerable<Review>> GetReviewsForReport()
        {
            return await this.reviewService.GetReviewsForReport();
        }

        [HttpGet("filter")]
        public async Task<IEnumerable<Review>> FilterReviewsByContent([FromQuery] string content)
        {
            return await this.reviewService.FilterReviewsByContent(content);
        }
    }
}
