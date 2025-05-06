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

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
            this.authenticationService = new AuthenticationService();
        }

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

        public async Task<User> GetUserById(Guid userId)
        {
            try
            {
                var user = await userRepository.GetUserById(userId);
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

        public async Task<User> GetUserByUsername(string username)
        {
            try
            {
                var user = await userRepository.GetUserByUsername(username);
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

        public async Task<User> GetCurrentUser()
        {
            Guid currentSessionId = App.CurrentSessionId;
            if (currentSessionId == Guid.Empty)
            {
                throw new InvalidOperationException(NoUserLoggedInMessage);
            }
            return await authenticationService.GetUser(currentSessionId);
        }

        public async Task<bool> ValidateAction(Guid userId, string resource, string action)
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
                return await userRepository.ValidateAction(userId, resource, action);
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

        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await userRepository.GetAllUsers();
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve all users.", ex);
            }
        }

        public async Task<List<User>> GetActiveUsersByRoleType(RoleType roleType)
        {
            try
            {
                return roleType switch
                {
                    > 0 => await userRepository.GetUsersByRoleType(roleType),
                    _ => throw new ArgumentException("Role type must be a valid value")
                };
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new UserServiceException($"Failed to get active users with role type '{roleType}'", ex);
            }
        }

        public async Task<List<User>> GetBannedUsers()
        {
            try
            {
                return await userRepository.GetUsersByRoleType(RoleType.Banned);
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to get banned users", ex);
            }
        }

        public async Task<List<User>> GetUsersByRoleType(RoleType roleType)
        {
            try
            {
                return await userRepository.GetUsersByRoleType(roleType);
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to retrieve users by role type '{roleType}'.", ex);
            }
        }

        public async Task<string> GetUserFullNameById(Guid userId)
        {
            try
            {
                User user = await userRepository.GetUserById(userId);
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

        public async Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                return await userRepository.GetBannedUsersWhoHaveSubmittedAppeals();
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve banned users who have submitted appeals.", ex);
            }
        }

        public async Task<RoleType> GetHighestRoleTypeForUser(Guid userId)
        {
            try
            {
                return await userRepository.GetRoleTypeForUser(userId);
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to retrieve the highest role type for user with ID {userId}.", ex);
            }
        }

        public async Task<List<User>> GetAdminUsers()
        {
            try
            {
                return await userRepository.GetUsersByRoleType(RoleType.Admin);
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve admin users.", ex);
            }
        }

        public async Task<List<User>> GetRegularUsers()
        {
            try
            {
                return await userRepository.GetUsersByRoleType(RoleType.User);
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve regular users.", ex);
            }
        }

        public async Task<List<User>> GetManagers()
        {
            try
            {
                return await userRepository.GetUsersByRoleType(RoleType.Manager);
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve manager users.", ex);
            }
        }

        public async Task UpdateUserRole(Guid userId, RoleType roleType)
        {
            try
            {
                User? user = await userRepository.GetUserById(userId);
                if (roleType == RoleType.Banned)
                {
                    bool hasBannedRole = false;
                    if (user.AssignedRole == RoleType.Banned)
                    {
                        hasBannedRole = true;
                    }
                    if (!hasBannedRole)
                    {
                        await userRepository.ChangeRoleToUser(userId, new Role(RoleType.Banned, "Banned"));
                    }
                }
                else
                {
                    await userRepository.ChangeRoleToUser(userId, new Role(roleType, roleType.ToString()));
                }

                // Update the user after modifying roles
                await userRepository.UpdateUser(user);
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new UserServiceException($"Failed to update role to {roleType} for user with ID {userId}", ex);
            }
        }
    }
}