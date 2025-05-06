using DataAccess.Model.AdminDashboard;

namespace DataAccess.Model.Authentication
{
    public class User
    {
        public User(Guid userId, string emailAddress, string userName, int numberOfDeletedReviews, bool hasSubmittedAppeal, RoleType assignedRole, string fullName)
        {
            this.UserId = userId;
            this.EmailAddress = emailAddress;
            this.Username = userName;
            this.NumberOfDeletedReviews = numberOfDeletedReviews;
            this.HasSubmittedAppeal = hasSubmittedAppeal;
            this.AssignedRole = assignedRole;
            this.FullName = fullName;
        }
        public User() { AssignedRole = RoleType.User; }
        public required Guid UserId { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public required string? TwoFASecret { get; set; }

        public string EmailAddress { get; set; }

        public int NumberOfDeletedReviews { get; set; }

        public bool HasSubmittedAppeal { get; set; }

        public RoleType AssignedRole { get; set; }
        public string FullName { get; set; }

        public void Returns(User user)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "Id: " + UserId.ToString() + ", email: " + EmailAddress;
        }
    }
}
