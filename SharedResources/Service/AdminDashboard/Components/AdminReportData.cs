using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;

public class AdminReportData
{
    public AdminReportData(DateTime reportDate, List<User> adminUsers, int activeUsersCount, int bannedUsersCount, int newReviewsCount, double averageRating, List<Review> recentReviews)
    {
        this.ReportDate = reportDate;
        this.AdminUsers = adminUsers;
        this.ActiveUsersCount = activeUsersCount;
        this.BannedUsersCount = bannedUsersCount;
        this.NewReviewsCount = newReviewsCount;
        this.AverageRating = averageRating;
        this.RecentReviews = recentReviews;
    }

    public DateTime ReportDate { get; set; }

    public List<User> AdminUsers { get; set; }

    public int ActiveUsersCount { get; set; }

    public int BannedUsersCount { get; set; }

    public int NewReviewsCount { get; set; }

    public double AverageRating { get; set; }

    public List<Review> RecentReviews { get; set; }
}
