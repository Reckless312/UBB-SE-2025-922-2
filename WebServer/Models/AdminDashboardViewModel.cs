using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DataAccess.Model.AutoChecker;


namespace WebServer.Models
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<Review> Reviews { get; set; }
        public IEnumerable<UpgradeRequest> UpgradeRequests { get; set; }
        public IEnumerable<string> OffensiveWords { get; set; }
        public IEnumerable<User> AppealingUsers { get; set; }
        public string searchBarContent { get; set; } = string.Empty;
        public IEnumerable<User> AppealsList { get; set; }  //  pending appeals (banned users who appealed)
        public IEnumerable<AppealDetailsViewModel> AppealsWithDetails { get; set; }
    }
}
