

using Microsoft.EntityFrameworkCore;
using SharedResources.Model.AdminDashboard;
using SharedResources.Model.Authentication;

namespace Server.Data
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UpgradeRequest> UpgradeRequests => Set<UpgradeRequest>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<User> Users => Set<User>();
    }
}
