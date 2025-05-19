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
            return await reviewService.GetAllReviews();
        }

        [HttpGet("since")]
        public async Task<IEnumerable<Review>> GetReviewsSince([FromQuery] DateTime date)
        {
            return await reviewService.GetReviewsSince(date);
        }
        
        [HttpGet("averageRatingVisibleReviews")]
        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            return await reviewService.GetAverageRatingForVisibleReviews();
        }
        
        [HttpGet("mostRecent")]
        public async Task<IEnumerable<Review>> GetMostRecentReviews([FromQuery] int count)
        {
            return await reviewService.GetMostRecentReviews(count);
        }
        
        [HttpGet("countAfterDate")]
        public async Task<int> GetReviewCountAfterDate([FromQuery] DateTime date)
        {
            return await reviewService.GetReviewCountAfterDate(date);
        }

        [HttpGet("flagged")]
        public async Task<IEnumerable<Review>> GetFlaggedReviews([FromQuery] int minFlags)
        {
            return await reviewService.GetFlaggedReviews(minFlags);
        }

        [HttpGet("byUser")]
        public async Task<IEnumerable<Review>> GetReviewsByUser([FromQuery] Guid userId)
        {
            return await reviewService.GetReviewsByUser(userId);
        }
        
        [HttpGet("{id}")]
        public async Task<Review> GetReviewById(int id)
        {
            return await reviewService.GetReviewById(id);
        }

        [HttpPatch("{id}/updateFlags")]
        public async Task UpdateNumberOfFlagsForReview(int id, [FromBody] int numberOfFlags)
        {
            await reviewService.UpdateNumberOfFlagsForReview(id, numberOfFlags);
        }

        [HttpPatch("{id}/updateVisibility")]
        public async Task UpdateReviewVisibility(int id, [FromBody] bool isHidden)
        {
            await reviewService.UpdateReviewVisibility(id, isHidden);
        }

        [HttpPost("add")]
        public async Task<int> AddReview([FromBody] Review review)
        {
            return await reviewService.AddReview(review);
        }

        [HttpDelete("{id}/delete")]
        public async Task RemoveReviewById(int id)
        {
            await reviewService.RemoveReviewById(id);
        }

        [HttpGet("hidden")]
        public async Task<IEnumerable<Review>> GetHiddenReviews()
        {
            return await reviewService.GetHiddenReviews();
        }

        [HttpGet("report")]
        public async Task<IEnumerable<Review>> GetReviewsForReport()
        {
            return await reviewService.GetReviewsForReport();
        }

        [HttpGet("filter")]
        public async Task<IEnumerable<Review>> FilterReviewsByContent([FromQuery] string content)
        {
            return await reviewService.FilterReviewsByContent(content);
        }
    }
}
