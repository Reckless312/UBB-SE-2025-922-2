// <copyright file="IUserService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
namespace DataAccess.Service.AdminDashboard.Interfaces
{
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task<List<User>> GetAllUsers();

        Task<User> GetUserByUsername(string username);

        Task<User> GetCurrentUser();

        Task<List<User>> GetUsersByRoleType(RoleType roleType);

        Task<List<User>> GetActiveUsersByRoleType(RoleType roleType);

        Task<List<User>> GetBannedUsers();

        Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals();

        Task<List<User>> GetAdminUsers();

        Task<List<User>> GetManagers();

        Task<List<User>> GetRegularUsers();

        Task<User> GetUserById(Guid id);

        Task<RoleType> GetHighestRoleTypeForUser(Guid id);

        Task UpdateUserRole(Guid userId, RoleType roleType);

        Task<string> GetUserFullNameById(Guid userId);

        Task<bool> ValidateAction(Guid userId, string resource, string action);

        void LogoutUser();
        void UpdateUserAppleaed(User user, bool newValue);
        Task ChangeRoleToUser(Guid userId, Role roleToAdd);
        Task<bool> UpdateUser(User user);
        Task<bool> CreateUser(User user);
        Task<List<User>> GetUsersWhoHaveSubmittedAppeals();
    }
}