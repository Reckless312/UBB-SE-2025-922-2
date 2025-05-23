using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using IRepository;
using DataAccess.Service.AdminDashboard.Components;
using DataAccess.Service.AdminDashboard.Interfaces;
using DataAccess.Service.Authentication;
using Repository.AdminDashboard;
using DataAccess.Service.Authentication.Interfaces;
using ServerAPI.Data;

namespace DataAccess.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IAuthenticationService authenticationService;
        public static Guid currentSessionId { set; private get; }

        private const string UserNotFoundMessage = "User not found";
        private const string NoUserLoggedInMessage = "No user is currently logged in.";
        private const string NullResourceError = "Resource cannot be null or empty.";
        private const string NullActionError = "Action cannot be null or empty.";
        
        public UserService(IUserRepository userRepository, IAuthenticationService authService)
        {
            this.userRepository = userRepository;
            this.authenticationService = authService;
        }

        //public UserService()
        //{
        //    this.userRepository = userRepository;
        //    this.authenticationService = authService;
        //}

        //// Constructor for dependency injection and testing
        //public UserService(IUserService repository, AuthenticationService authService)
        //{
        //    userRepository = repository;
        //    authenticationService = authService;
        //}
        public void SetCurrentSession(Guid sessionId) { currentSessionId = sessionId; }
        public async Task<User> GetUserById(Guid userId)
        {
            try
            {
                User? user = await userRepository.GetUserById(userId);
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
                User? user = await userRepository.GetUserByUsername(username);
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
            if (currentSessionId == Guid.Empty)
            {
                throw new InvalidOperationException(NoUserLoggedInMessage);
            }
            return await authenticationService.GetUser(currentSessionId);
        }

        //public async Task<bool> ValidateAction(Guid userId, string resource, string action)
        //{
        //    if (string.IsNullOrEmpty(resource))
        //    {
        //        throw new ArgumentException(NullResourceError, nameof(resource));
        //    }

        //    if (string.IsNullOrEmpty(action))
        //    {
        //        throw new ArgumentException(NullActionError, nameof(action));
        //    }

        //    try
        //    {
        //        return await userRepository.ValidateAction(userId, resource, action);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new UserServiceException($"Failed to validate action '{action}' on resource '{resource}' for user with ID {userId}.", ex);
        //    }
        //}

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
                if (user == null)
                {
                    throw new UserServiceException($"User with ID {userId} not found", new ArgumentException($"User with ID {userId} not found"));
                }

                if (roleType == RoleType.Banned)
                {
                    bool hasBannedRole = false;
                    if (user.AssignedRole == RoleType.Banned)
                    {
                        hasBannedRole = true;
                    }
                    if (!hasBannedRole)
                    {
                        user.AssignedRole = RoleType.Banned;
                    }
                }
                else
                {
                    user.AssignedRole = roleType;
                }
                await userRepository.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to update role for user with ID {userId}", ex);
            }
        }

        public async Task UpdateUserAppleaed(User user, bool newValue)
        {
            try
            {
                user.HasSubmittedAppeal = newValue;
                await userRepository.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to update appeal status for user {user.UserId}", ex);
            }
        }

        public Task<bool> ValidateAction(Guid userId, string resource, string action)
        {
            throw new NotImplementedException();
        }

        public async Task ChangeRoleToUser(Guid userId, Role role)
        {
            try
            {
                await userRepository.ChangeRoleToUser(userId, role);
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to change role for user with ID {userId}.", ex);
            }
        }
        public async Task<bool> CreateUser(User user)
        {
            try
            {
                return await userRepository.CreateUser(user);
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to create user.", ex);
            }
        }
        public async Task<bool> UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                return await userRepository.UpdateUser(user);
            }
            catch (Exception ex)
            {
                throw new UserServiceException($"Failed to update user with ID {user.UserId}.", ex);
            }
        }
        public async Task<List<User>> GetUsersWhoHaveSubmittedAppeals()
        {
            try
            {
                return await userRepository.GetUsersWhoHaveSubmittedAppeals();
            }
            catch (Exception ex)
            {
                throw new UserServiceException("Failed to retrieve users who have submitted appeals.", ex);
            }
        }

        

    }
}