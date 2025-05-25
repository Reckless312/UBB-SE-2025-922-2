using DataAccess.Model.Authentication;
using DataAccess.Model.AdminDashboard;
namespace WebServer.Models
{
    public class AppealDetailsViewModel
    {
        public required User User { get; set; }
        public required IEnumerable<Review> Reviews { get; set; }
    }
}