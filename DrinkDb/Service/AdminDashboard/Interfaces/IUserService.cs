// <copyright file="IUserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides user-related operations for retrieving and modifying user data.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets all users in the system.
        /// </summary>
        /// <returns>A list of all users.</returns>
        Task<List<User>> GetAllUsers();

        /// <summary>
        /// Gets a user by username.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The user with the matching username.</returns>
        Task<User> GetUserByUsername(string username);

        /// <summary>
        /// Gets the current logged-in user.
        /// </summary>
        /// <returns>The current user.</returns>
        Task<User> GetCurrentUser();

        /// <summary>
        /// Gets all users with the specified role type.
        /// </summary>
        /// <param name="roleType">The role type to filter by.</param>
        /// <returns>A list of users matching the role type.</returns>
        Task<List<User>> GetUsersByRoleType(RoleType roleType);

        /// <summary>
        /// Gets all active users with the specified role type.
        /// </summary>
        /// <param name="roleType">The role type to filter by.</param>
        /// <returns>A list of active users matching the role type.</returns>
        Task<List<User>> GetActiveUsersByRoleType(RoleType roleType);

        /// <summary>
        /// Gets all banned users.
        /// </summary>
        /// <returns>A list of banned users.</returns>
        Task<List<User>> GetBannedUsers();

        /// <summary>
        /// Gets banned users who have submitted appeals.
        /// </summary>
        /// <returns>A list of banned users with submitted appeals.</returns>
        Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals();

        /// <summary>
        /// Gets all users with an admin role.
        /// </summary>
        /// <returns>A list of admin users.</returns>
        Task<List<User>> GetAdminUsers();

        /// <summary>
        /// Gets all users with a manager role.
        /// </summary>
        /// <returns>A list of manager users.</returns>
        Task<List<User>> GetManagers();

        /// <summary>
        /// Gets all regular users (non-admin, non-manager).
        /// </summary>
        /// <returns>A list of regular users.</returns>
        Task<List<User>> GetRegularUsers();

        /// <summary>
        /// Gets a user by ID.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The user with the specified ID.</returns>
        Task<User> GetUserById(Guid id);

        /// <summary>
        /// Gets the highest role type assigned to a user.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>The highest role type.</returns>
        Task<RoleType> GetHighestRoleTypeForUser(Guid id);

        /// <summary>
        /// Updates a user's role.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="roleType">The new role type to assign.</param>
        Task UpdateUserRole(Guid userId, RoleType roleType);

        /// <summary>
        /// Gets the full name of a user by their ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The full name of the user.</returns>
        Task<string> GetUserFullNameById(Guid userId);

        /// <summary>
        /// Validates if a user can perform an action on a resource.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="resource">The resource to check.</param>
        /// <param name="action">The action to check.</param>
        /// <returns>True if the user can perform the action, false otherwise.</returns>
        Task<bool> ValidateAction(Guid userId, string resource, string action);

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        void LogoutUser();
        void UpdateUserAppleaed(User user, bool newValue);
    }
}