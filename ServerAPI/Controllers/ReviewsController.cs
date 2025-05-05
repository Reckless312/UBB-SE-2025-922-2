using Microsoft.AspNetCore.Mvc;
using DataAccess.Model.AdminDashboard;
using IRepository;
using DrinkDb_Auth.Repository.AdminDashboard;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("reviews")]
    public class ReviewsController : ControllerBase
    {
        private IReviewsRepository repository;

        public ReviewsController(IReviewsRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet("")]
        public async Task<IEnumerable<Review>> GetAll()
        {
            return repository.GetAllReviews().Result;
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
        public IEnumerable<Review> GetFlaggedReviews([FromQuery] int minFlags)
        {
            return  repository.GetFlaggedReviews(minFlags).Result;
        }

        [HttpGet("byUser")]
        public async Task<IEnumerable<Review>> GetReviewsByUser([FromQuery] Guid userId)
        {
            return await repository.GetReviewsByUser(userId);
        }
        
        [HttpGet("{id}")]
        public async Task<Review> GetReviewById(int id)
        {
            return  repository.GetReviewById(id).Result;
        }

        [HttpPatch("{id}/updateFlags")]
        public async Task UpdateNumberOfFlagsForReview(int id, [FromBody] int numberOfFlags)
        {
             repository.UpdateNumberOfFlagsForReview(id, numberOfFlags);
        }

        [HttpPatch("{id}/updateVisibility")]
        public async Task UpdateReviewVisibility(int id, [FromBody] bool isHidden)
        {
            repository.UpdateReviewVisibility(id, isHidden);
        }

        [HttpPost("add")]
        public async Task<int> AddReview([FromBody] Review review)
        {
            return await repository.AddReview(review);
        }

        [HttpDelete("{id}/delete")]
        public async Task RemoveReviewById(int id)
        {
            repository.RemoveReviewById(id);
        }

        [HttpGet("hidden")]
        public async Task<IEnumerable<Review>> GetHiddenReviews()
        {
            return await repository.GetHiddenReviews();
        }
    }
}
