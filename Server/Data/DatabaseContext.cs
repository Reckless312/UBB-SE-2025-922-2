using Microsoft.EntityFrameworkCore;
using DataAccess.Model.Authentication;
using DataAccess.Model.AdminDashboard;

namespace Server.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<UpgradeRequest> UpgradeRequests => Set<UpgradeRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configure entities
        }
    }
}