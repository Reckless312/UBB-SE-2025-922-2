namespace DrinkDb_Auth.ProxyRepository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    public class ReviewsProxyRepository : IReviewsRepository
    {
        public int AddReview(Review review)
        {
            throw new NotImplementedException();
        }

        public List<Review> GetAllReviews()
        {
            throw new NotImplementedException();
        }

        public double GetAverageRatingForVisibleReviews()
        {
            throw new NotImplementedException();
        }

        public List<Review> GetFlaggedReviews(int minFlags)
        {
            throw new NotImplementedException();
        }

        public List<Review> GetHiddenReviews()
        {
            throw new NotImplementedException();
        }

        public List<Review> GetMostRecentReviews(int count)
        {
            throw new NotImplementedException();
        }

        public Review GetReviewById(int reviewID)
        {
            throw new NotImplementedException();
        }

        public int GetReviewCountAfterDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<Review> GetReviewsByUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public List<Review> GetReviewsSince(DateTime date)
        {
            throw new NotImplementedException();
        }

        public bool RemoveReviewById(int reviewID)
        {
            throw new NotImplementedException();
        }

        public void UpdateNumberOfFlagsForReview(int reviewID, int numberOfFlags)
        {
            throw new NotImplementedException();
        }

        public void UpdateReviewVisibility(int reviewID, bool isHidden)
        {
            throw new NotImplementedException();
        }
    }
}
