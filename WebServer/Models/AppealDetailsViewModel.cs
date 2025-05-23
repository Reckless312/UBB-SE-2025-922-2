// File: Models/AppealDetailsViewModel.cs
using DataAccess.Model.Authentication;   // for User
using DataAccess.Model.AdminDashboard;   // for Review
namespace WebServer.Models
{
    public class AppealDetailsViewModel
    {
        public User User { get; set; }               // The banned user who appealed
        public IEnumerable<Review> Reviews { get; set; }  // That user’s reviews (for context)
    }
}
