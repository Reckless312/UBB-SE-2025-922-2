using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using Repository.AdminDashboard;
using IRepository;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReviewsController : ControllerBase
    {
        private IReviewsRepository repository;

        public ReviewsController(IReviewsRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet]
        public async Task<IEnumerable<Review>> GetAll()
        {
            return await repository.GetAllReviews();
        }

        [HttpGet("since")]
        public async Task<IEnumerable<Review>> GetReviewsSince([FromQuery] DateTime date)
        {
            return await repository.GetReviewsSince(date);
        }
        
        [HttpGet("averageRatingVisibleReviews")]
        public async Task<double> GetAverageRatingForVisibleReviews()
        {
            return await repository.GetAverageRatingForVisibleReviews();
        }
        
        [HttpGet("mostRecent")]
        public async Task<IEnumerable<Review>> GetMostRecentReviews([FromQuery] int count)
        {
            return await repository.GetMostRecentReviews(count);
        }
        
        [HttpGet("countAfterDate")]
        public async Task<double> GetReviewCountAfterDate([FromQuery] DateTime date)
        {
            return await repository.GetReviewCountAfterDate(date);
        }

        [HttpGet("flagged")]
        public async Task<IEnumerable<Review>> GetFlaggedReviews([FromQuery] int minFlags)
        {
            return await repository.GetFlaggedReviews(minFlags);
        }

        [HttpGet("byUser")]
        public async Task<IEnumerable<Review>> GetReviewsByUser([FromQuery] Guid userId)
        {
            return await repository.GetReviewsByUser(userId);
        }
        
        [HttpGet("{id}")]
        public async Task<Review> GetReviewById(int id)
        {
            return await repository.GetReviewById(id);
        }

        [HttpPatch("{id}/updateFlags")]
        public async Task UpdateNumberOfFlagsForReview(int id, [FromQuery] int numberOfFlags)
        {
            await repository.UpdateNumberOfFlagsForReview(id, numberOfFlags);
        }

        [HttpPatch("{id}/updateVisibility")]
        public async Task UpdateReviewVisibility(int id, [FromQuery] bool isHidden)
        {
            await repository.UpdateReviewVisibility(id, isHidden);
        }

        [HttpPost("add")]
        public async Task<int> AddReview([FromBody] Review review)
        {
            return await repository.AddReview(review);
        }

        [HttpDelete("{id}/delete")]
        public async Task<bool> RemoveReviewById(int id)
        {
            return await repository.RemoveReviewById(id);
        }

        [HttpGet("hidden")]
        public async Task<IEnumerable<Review>> GetHiddenReviews()
        {
            return await repository.GetHiddenReviews();
        }
    }
}
