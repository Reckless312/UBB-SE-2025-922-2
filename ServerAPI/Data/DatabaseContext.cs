using Microsoft.EntityFrameworkCore;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using System.Collections.Generic;
using System.Reflection.Emit;
using Azure.Core;
using ServerAPI.Controllers;
using DataAccess.Model.AutoChecker;

namespace ServerAPI.Data
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

        public DbSet<OffensiveWord> OffensiveWords => Set<OffensiveWord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
 
            // configure user
            modelBuilder.Entity<User>(user =>
            {
                user.HasKey(currentUser => currentUser.UserId);
                user.Property(currentUser => currentUser.Username).IsRequired().HasMaxLength(50);
                user.Property(currentUser => currentUser.PasswordHash).IsRequired();
                user.Property(currentUser => currentUser.TwoFASecret).IsRequired(false);
                user.Property(currentUser => currentUser.EmailAddress).IsRequired(false);
                user.Property(currentUser => currentUser.NumberOfDeletedReviews).IsRequired();
                user.Property(currentUser => currentUser.HasSubmittedAppeal).IsRequired();
                user.HasMany(currentUser => currentUser.AssignedRoles)
                         .WithMany();
            });

            // configure role
            modelBuilder.Entity<Role>(role =>
            {
                role.HasKey(currentRole => currentRole.RoleType);
                role.Property(currentRole => currentRole.RoleName).IsRequired().HasMaxLength(10);
            });

            // configure upgrade request
            modelBuilder.Entity<UpgradeRequest>(request =>
            {
                request.HasKey(currentRequest => currentRequest.UpgradeRequestId);
                request.HasOne<User>().WithMany().HasForeignKey(upgradeRequest=> upgradeRequest.RequestingUserIdentifier);
                request.Property(upgradeRequest=>upgradeRequest.RequestingUserIdentifier).IsRequired();
            });

            // configure session
            modelBuilder.Entity<Session>(session =>
            {
                session.HasKey(currentSession => currentSession.SessionId);
                session.HasOne<User>().WithMany().HasForeignKey(currentSession => currentSession.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // configure review
            modelBuilder.Entity<Review>(review =>
            {
                review.HasKey(currentReview => currentReview.ReviewId);
                review.HasOne<User>().WithMany().HasForeignKey(currentRewview => currentRewview.UserId);
                review.Property(currentReview => currentReview.Rating).IsRequired();
                review.Property(currentReview => currentReview.Content).IsRequired();
                review.Property(currentReview => currentReview.CreatedDate).IsRequired();
                review.Property(currentReview => currentReview.NumberOfFlags).IsRequired();
                review.Property(currentReview => currentReview.IsHidden).IsRequired();
            });

            // configure offensive words
            modelBuilder.Entity<OffensiveWord>(word =>
            {
                word.HasKey(offensiveWord => offensiveWord.OffensiveWordId);
                word.Property(offensiveWord => offensiveWord.Word);
            });
        }
    }
}
