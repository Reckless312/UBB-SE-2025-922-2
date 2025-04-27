namespace App1.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using App1.Models;

    /// <summary>
    /// Repository for managing user data and operations.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>
        /// Static role collections for user initialization.
        /// </summary>
        public static List<Role> BasicUserRoles = new List<Role> { new Role(RoleType.User, "user") };

        public static List<Role> AdminRoles = new List<Role> { new Role(RoleType.User, "user"), new Role(RoleType.Admin, "admin") };

        public static List<Role> ManagerRoles = new List<Role> { new Role(RoleType.User, "user"), new Role(RoleType.Admin, "admin"), new Role(RoleType.Manager, "manager") };

        public static List<Role> BannedUserRoles = new List<Role> { new Role(RoleType.Banned, "banned") };

        /// <summary>
        /// Internal list of users managed by the repository.
        /// </summary>
        private readonly List<User> _usersList;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class with default user data.
        /// </summary>
        public UserRepository()
        {
            this._usersList = new List<User>
            {
                new User(userId: 1, emailAddress: "bianca.georgiana.cirnu@gmail.com", fullName: "Bianca Georgiana Cirnu", numberOfDeletedReviews: 2, hasSubmittedAppeal: true, assignedRoles: BasicUserRoles),
                new User(userId: 3, emailAddress: "admin.one@example.com", fullName: "Admin One", numberOfDeletedReviews: 0, hasSubmittedAppeal: false, assignedRoles: AdminRoles),
                new User(userId: 5, emailAddress: "admin.two@example.com", fullName: "Admin Two", numberOfDeletedReviews: 0, hasSubmittedAppeal: false, assignedRoles: AdminRoles),
            };
        }

        /// <summary>
        /// Retrieves all users who have submitted appeals.
        /// </summary>
        /// <returns>A list of users who have submitted appeals.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                if (this._usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return this._usersList.Where(user => user.HasSubmittedAppeal).ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve users who have submitted appeals.", ex);
            }
        }

        /// <summary>
        /// Retrieves all users with a specific role type.
        /// </summary>
        /// <param name="roleType">The role type to filter users by.</param>
        /// <returns>A list of users with the specified role type.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetUsersByRoleType(RoleType roleType)
        {
            try
            {
                if (this._usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return this._usersList.Where(user => user.AssignedRoles != null && user.AssignedRoles.Any(role => role.RoleType == roleType)).ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve users with role type '{roleType}'.", ex);
            }
        }

        /// <summary>
        /// Retrieves the highest role type assigned to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The highest role type assigned to the user.</returns>
        /// <exception cref="RepositoryException">Thrown when the user has no roles or does not exist.</exception>
        public RoleType GetHighestRoleTypeForUser(int userId)
        {
            try
            {
                User user = this.GetUserByID(userId);
                if (user.AssignedRoles == null || !user.AssignedRoles.Any())
                {
                    throw new ArgumentException($"No roles found for user with ID {userId}");
                }

                return user.AssignedRoles.Max(role => role.RoleType);
            }
            catch (ArgumentException ex)
            {
                throw new RepositoryException("User has no roles assigned.", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to get highest role type for user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The user with the specified ID.</returns>
        /// <exception cref="RepositoryException">Thrown when the user does not exist or an error occurs.</exception>
        public User GetUserByID(int userId)
        {
            try
            {
                User user = this._usersList.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    throw new ArgumentException($"No user found with ID {userId}");
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all banned users who have submitted appeals.
        /// </summary>
        /// <returns>A list of banned users who have submitted appeals.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                if (this._usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return this._usersList.Where(user =>
                    user.HasSubmittedAppeal &&
                    user.AssignedRoles != null &&
                    user.AssignedRoles.Any(role => role.RoleType == RoleType.Banned)
                ).ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve banned users who have submitted appeals.", ex);
            }
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        /// <param name="userId">The ID of the user to add the role to.</param>
        /// <param name="roleToAdd">The role to add to the user.</param>
        /// <exception cref="RepositoryException">Thrown when the user does not exist or an error occurs.</exception>
        public void AddRoleToUser(int userId, Role roleToAdd)
        {
            try
            {
                User user = this._usersList.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    throw new ArgumentException($"No user found with ID {userId}");
                }

                user.AssignedRoles.Add(roleToAdd);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to add role to user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
        /// <exception cref="RepositoryException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetAllUsers()
        {
            try
            {
                if (this._usersList == null)
                {
                    throw new NullReferenceException("_usersList is null.");
                }

                return this._usersList;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve all users.", ex);
            }
        }

        /// <summary>
        /// Exception class for repository-related errors.
        /// </summary>
        public class RepositoryException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RepositoryException"/> class.
            /// </summary>
            /// <param name="message">The error message.</param>
            /// <param name="innerException">The inner exception.</param>
            public RepositoryException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}