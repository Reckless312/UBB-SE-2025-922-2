using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;

namespace WebServer.Models
{
    public class UserPageModel
    {
        public User currentUser { get; set; }
        public IEnumerable<Review> currentUserReviews { get; set; } 
        public IEnumerable<string> currentUserDrinks { get; set; } // prepared for the merge
    }
}
