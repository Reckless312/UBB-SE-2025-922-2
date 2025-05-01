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
        private IReviewsRepository repository = new ReviewsRepository();

        [HttpGet]
        public IEnumerable<Review> GetAll()
        {
            return repository.GetAllReviews();
        }

        [HttpGet("since")]
        public IEnumerable<Review> GetReviewsSince([FromQuery] DateTime date)
        {
            return repository.GetReviewsSince(date);
        }
        
        [HttpGet("averageRatingVisibleReviews")]
        public double GetAverageRatingForVisibleReviews()
        {
            return repository.GetAverageRatingForVisibleReviews();
        }
        
        [HttpGet("mostRecent")]
        public IEnumerable<Review> GetMostRecentReviews([FromQuery] int count)
        {
            return repository.GetMostRecentReviews(count);
        }
        
        [HttpGet("countAfterDate")]
        public double GetReviewCountAfterDate([FromQuery] DateTime date)
        {
            return repository.GetReviewCountAfterDate(date);
        }

        [HttpGet("flagged")]
        public IEnumerable<Review> GetFlaggedReviews([FromQuery] int minFlags)
        {
            return repository.GetFlaggedReviews(minFlags);
        }

        [HttpGet("byUser")]
        public IEnumerable<Review> GetReviewsByUser([FromQuery] Guid userId)
        {
            return repository.GetReviewsByUser(userId);
        }
        
        [HttpGet("{id}")]
        public Review GetReviewById(int id)
        {
            return repository.GetReviewById(id);
        }

        [HttpPatch("{id}/updateFlags")]
        public void UpdateNumberOfFlagsForReview(int id, [FromQuery] int numberOfFlags)
        {
            repository.UpdateNumberOfFlagsForReview(id, numberOfFlags);
        }

        [HttpPatch("{id}/updateVisibility")]
        public void UpdateReviewVisibility(int id, [FromQuery] bool isHidden)
        {
            repository.UpdateReviewVisibility(id, isHidden);
        }

        [HttpPost("add")]
        public int AddReview([FromBody] Review review)
        {
            return repository.AddReview(review);
        }

        [HttpDelete("{id}/delete")]
        public bool RemoveReviewById(int id)
        {
            return repository.RemoveReviewById(id);
        }

        [HttpGet("hidden")]
        public IEnumerable<Review> GetHiddenReviews()
        {
            return repository.GetHiddenReviews();
        }
    }
}
