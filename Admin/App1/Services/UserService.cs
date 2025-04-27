namespace App1.Services
{
    using System;
    using System.Collections.Generic;
    using App1.Models;
    using App1.Repositories;
    using static App1.Repositories.UserRepository;

    /// <summary>
    /// Service for managing user-related operations.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository to interact with user data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="userRepository"/> is null.</exception>
        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of all users.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetAllUsers()
        {
            try
            {
                return this.userRepository.GetAllUsers();
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException("Failed to retrieve all users.", ex);
            }
        }

        /// <summary>
        /// Retrieves active users by their role type.
        /// </summary>
        /// <param name="roleType">The role type to filter users by.</param>
        /// <returns>A list of active users with the specified role type.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="roleType"/> is invalid.</exception>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetActiveUsersByRoleType(RoleType roleType)
        {
            try
            {
                return roleType switch
                {
                    > 0 => this.userRepository.GetUsersByRoleType(roleType),
                    _ => throw new ArgumentException("Permission ID must be positive")
                };
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException("Failed to get active users", ex);
            }
        }

        /// <summary>
        /// Retrieves all banned users.
        /// </summary>
        /// <returns>A list of banned users.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetBannedUsers()
        {
            try
            {
                return this.userRepository.GetUsersByRoleType(RoleType.Banned);
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException("Failed to get banned users", ex);
            }
        }

        /// <summary>
        /// Retrieves users by their role type.
        /// </summary>
        /// <param name="roleType">The role type to filter users by.</param>
        /// <returns>A list of users with the specified role type.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetUsersByRoleType(RoleType roleType)
        {
            try
            {
                return this.userRepository.GetUsersByRoleType(roleType);
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException($"Failed to retrieve users by role type '{roleType}'.", ex);
            }
        }

        /// <summary>
        /// Retrieves the full name of a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The full name of the user.</returns>
        /// <exception cref="UserServiceException">Thrown when the user does not exist or an error occurs.</exception>
        public string GetUserFullNameById(int userId)
        {
            try
            {
                User user = this.userRepository.GetUserByID(userId);
                if (user == null)
                {
                    throw new UserServiceException($"Failed to retrieve the full name of the user with ID {userId}.", new ArgumentNullException(nameof(user)));
                }

                return user.FullName;
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException($"Failed to retrieve the full name of the user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all banned users who have submitted appeals.
        /// </summary>
        /// <returns>A list of banned users who have submitted appeals.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                return this.userRepository.GetBannedUsersWhoHaveSubmittedAppeals();
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException("Failed to retrieve banned users who have submitted appeals.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The user with the specified ID.</returns>
        /// <exception cref="UserServiceException">Thrown when the user does not exist or an error occurs.</exception>
        public User GetUserById(int userId)
        {
            try
            {
                User user = this.userRepository.GetUserByID(userId);
                if (user == null)
                {
                    throw new UserServiceException($"Failed to retrieve user with ID {userId}.", new ArgumentNullException(nameof(user)));
                }

                return user;
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException($"Failed to retrieve user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves the highest role type assigned to a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The highest role type assigned to the user.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving the role type.</exception>
        public RoleType GetHighestRoleTypeForUser(int userId)
        {
            try
            {
                return this.userRepository.GetHighestRoleTypeForUser(userId);
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException($"Failed to retrieve the highest role type for user with ID {userId}.", ex);
            }
        }

        /// <summary>
        /// Retrieves all admin users.
        /// </summary>
        /// <returns>A list of admin users.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetAdminUsers()
        {
            try
            {
                return this.userRepository.GetUsersByRoleType(RoleType.Admin);
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException("Failed to retrieve admin users.", ex);
            }
        }

        /// <summary>
        /// Retrieves all regular users.
        /// </summary>
        /// <returns>A list of regular users.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetRegularUsers()
        {
            try
            {
                return this.userRepository.GetUsersByRoleType(RoleType.User);
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException("Failed to retrieve regular users.", ex);
            }
        }

        /// <summary>
        /// Retrieves all manager users.
        /// </summary>
        /// <returns>A list of manager users.</returns>
        /// <exception cref="UserServiceException">Thrown when an error occurs while retrieving users.</exception>
        public List<User> GetManagers()
        {
            try
            {
                return this.userRepository.GetUsersByRoleType(RoleType.Manager);
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException("Failed to retrieve manager users.", ex);
            }
        }

        /// <summary>
        /// Updates the role of a user.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="roleType">The new role type to assign to the user.</param>
        /// <exception cref="UserServiceException">Thrown when an error occurs while updating the user's role.</exception>
        public void UpdateUserRole(int userId, RoleType roleType)
        {
            try
            {
                User user = this.userRepository.GetUserByID(userId);
                if (user == null)
                {
                    return;
                }

                if (roleType == RoleType.Banned)
                {
                    bool hasBannedRole = false;
                    foreach (Role role in user.AssignedRoles)
                    {
                        if (role.RoleType == RoleType.Banned)
                        {
                            hasBannedRole = true;
                            break;
                        }
                    }

                    if (!hasBannedRole)
                    {
                        user.AssignedRoles.Clear();
                        this.userRepository.AddRoleToUser(userId, new Role(RoleType.Banned, "Banned"));
                    }
                }
                else
                {
                    user.AssignedRoles.Clear();
                    this.userRepository.AddRoleToUser(userId, new Role(RoleType.User, "User"));
                }
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to update user role", ex);
            }
        }
    }
}