using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharedResources.Model.AdminDashboard;
using SharedResources.Model.Authentication;
using DrinkDb_Auth.Repository.AdminDashboard;
using SharedResources.Repository.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.AdminDashboard.Components;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.Authentication;
using static DrinkDb_Auth.Repository.AdminDashboard.UserRepository;

namespace DrinkDb_Auth.Service
{
    public class UserService: IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly AuthenticationService authenticationService;

        private const string UserNotFoundMessage = "User not found";
        private const string NoUserLoggedInMessage = "No user is currently logged in.";
        private const string NullResourceError = "Resource cannot be null or empty.";
        private const string NullActionError = "Action cannot be null or empty.";

        public UserService()
        {
            // Aici trebuie sa vina proxy ul in final, da momentan se poate testa pe repo ul facut
            userRepository = new UserRepository();
            authenticationService = new AuthenticationService();
        }

        public User GetUserById(Guid userId)
        {
            return userRepository.GetUserById(userId) ?? throw new ArgumentException(UserNotFoundMessage, nameof(userId));
        }

        public User GetUserByUsername(string username)
        {
            return userRepository.GetUserByUsername(username) ?? throw new ArgumentException(UserNotFoundMessage, nameof(username));
        }

        public User GetCurrentUser()
        {
            Guid currentSessionId = App.CurrentSessionId;
            if (currentSessionId == Guid.Empty)
            {
                throw new InvalidOperationException(NoUserLoggedInMessage);
            }
            return authenticationService.GetUser(currentSessionId);
        }

        public bool ValidateAction(Guid userId, string resource, string action)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentException(NullResourceError, nameof(resource));
            }
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentException(NullActionError, nameof(action));
            }
            return userRepository.ValidateAction(userId, resource, action);
        }

        public void LogoutUser()
        {
            authenticationService.Logout();
        }

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

        public string GetUserFullNameById(Guid userId)
        {
            try
            {
                User user = this.userRepository.GetUserById(userId);
                if (user == null)
                {
                    throw new UserServiceException($"Failed to retrieve the full name of the user with ID {userId}.", new ArgumentNullException(nameof(user)));
                }

                return user.Username;
            }
            catch (RepositoryException ex)
            {
                throw new UserServiceException($"Failed to retrieve the full name of the user with ID {userId}.", ex);
            }
        }

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

        public RoleType GetHighestRoleTypeForUser(Guid userId)
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

        public void UpdateUserRole(Guid userId, RoleType roleType)
        {
            try
            {
                User? user = this.userRepository.GetUserById(userId);
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