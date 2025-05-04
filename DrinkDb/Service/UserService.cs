using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using DrinkDb_Auth.ProxyRepository.AdminDashboard;
using IRepository;
using DrinkDb_Auth.Service.AdminDashboard.Components;
using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
using DrinkDb_Auth.Service.Authentication;
using Repository.AdminDashboard;

namespace DrinkDb_Auth.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly AuthenticationService authenticationService;

        private const string UserNotFoundMessage = "User not found";
        private const string NoUserLoggedInMessage = "No user is currently logged in.";
        private const string NullResourceError = "Resource cannot be null or empty.";
        private const string NullActionError = "Action cannot be null or empty.";

        public UserService()
        {
            this.userRepository = new UserProxyRepository();
            this.authenticationService = new AuthenticationService();
        }

        // Constructor for dependency injection and testing
        public UserService(IUserRepository repository, AuthenticationService authService)
        {
            userRepository = repository;
            authenticationService = authService;
        }

        public User GetUserById(Guid userId)
        {
            try
            {
                var user = userRepository.GetUserById(userId).Result;
                if (user == null)
                {
                    throw new ArgumentException(UserNotFoundMessage, nameof(userId));
                }
                return user;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new UserServiceException($"Failed to retrieve user with ID {userId}.", ex);
            }
        }

        public User GetUserByUsername(string username)
        {
            try
            {
                var user = userRepository.GetUserByUsername(username).Result;
                if (user == null)
                {
                    throw new ArgumentException(UserNotFoundMessage, nameof(username));
                }
                return user;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new UserServiceException($"Failed to retrieve user with username {username}.", ex);
            }
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

            try
            {
                return userRepository.ValidateAction(userId, resource, action).Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to validate action '{action}' on resource '{resource}' for user with ID {userId}.", ex);
            }
        }

        public void LogoutUser()
        {
            authenticationService.Logout();
        }

        public List<User> GetAllUsers()
        {
            try
            {
                return userRepository.GetAllUsers().Result;
            }
            catch (Exception ex)
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
                    > 0 => userRepository.GetUsersByRoleType(roleType).Result,
                    _ => throw new ArgumentException("Role type must be a valid value")
                };
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new UserServiceException($"Failed to get active users with role type '{roleType}'", ex);
            }
        }

        public List<User> GetBannedUsers()
        {
            try
            {
                return userRepository.GetUsersByRoleType(RoleType.Banned).Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to get banned users", ex);
            }
        }

        public List<User> GetUsersByRoleType(RoleType roleType)
        {
            try
            {
                return userRepository.GetUsersByRoleType(roleType).Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to retrieve users by role type '{roleType}'.", ex);
            }
        }

        public string GetUserFullNameById(Guid userId)
        {
            try
            {
                User user = userRepository.GetUserById(userId).Result;
                if (user == null)
                {
                    throw new UserServiceException($"Failed to retrieve the full name of the user with ID {userId}.", 
                        new ArgumentNullException(nameof(user)));
                }

                return user.Username;
            }
            catch (Exception ex) when (!(ex is UserServiceException))
            {
                throw new UserServiceException($"Failed to retrieve the full name of the user with ID {userId}.", ex);
            }
        }

        public List<User> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                return userRepository.GetBannedUsersWhoHaveSubmittedAppeals().Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve banned users who have submitted appeals.", ex);
            }
        }

        public RoleType GetHighestRoleTypeForUser(Guid userId)
        {
            try
            {
                return userRepository.GetHighestRoleTypeForUser(userId).Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to retrieve the highest role type for user with ID {userId}.", ex);
            }
        }

        public List<User> GetAdminUsers()
        {
            try
            {
                return userRepository.GetUsersByRoleType(RoleType.Admin).Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve admin users.", ex);
            }
        }

        public List<User> GetRegularUsers()
        {
            try
            {
                return userRepository.GetUsersByRoleType(RoleType.User).Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve regular users.", ex);
            }
        }

        public List<User> GetManagers()
        {
            try
            {
                return userRepository.GetUsersByRoleType(RoleType.Manager).Result;
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve manager users.", ex);
            }
        }

        public void UpdateUserRole(Guid userId, RoleType roleType)
        {
            try
            {
                User? user = userRepository.GetUserById(userId).Result;
                if (user == null)
                {
                    throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
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
                        userRepository.AddRoleToUser(userId, new Role(RoleType.Banned, "Banned"));
                    }
                }
                else
                {
                    user.AssignedRoles.Clear();
                    userRepository.AddRoleToUser(userId, new Role(roleType, roleType.ToString()));
                }

                // Update the user after modifying roles
                userRepository.UpdateUser(user);
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new UserServiceException($"Failed to update role to {roleType} for user with ID {userId}", ex);
            }
        }
    }
}