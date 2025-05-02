using DataAccess.Model.AdminDashboard;

namespace DataAccess.Model.Authentication
{
    public class User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        public User()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class with specified details.
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="emailAddress">The email address of the user.</param>
        /// <param name="fullName">The full name of the user.</param>
        /// <param name="numberOfDeletedReviews">The number of reviews deleted by the user.</param>
        /// <param name="hasSubmittedAppeal">Indicates whether the user has submitted an appeal.</param>
        /// <param name="assignedRoles">The list of roles assigned to the user.</param>
        public User(Guid userId, string emailAddress, string userName, int numberOfDeletedReviews, bool hasSubmittedAppeal, List<Role> assignedRoles)
        {
            this.UserId = userId;
            this.EmailAddress = emailAddress;
            this.Username = userName;
            this.NumberOfDeletedReviews = numberOfDeletedReviews;
            this.HasSubmittedAppeal = hasSubmittedAppeal;
            this.AssignedRoles = assignedRoles;
        }

        public required Guid UserId { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public required string? TwoFASecret { get; set; }

        public string EmailAddress { get; set; }

        public int NumberOfDeletedReviews { get; set; }

        public bool HasSubmittedAppeal { get; set; }

        public List<Role> AssignedRoles { get; set; }
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
