// <copyright file="IUserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;

    public interface IUserRepository
    {
        // public void UpdateRole(int userID, int permissionID);
        public Task<List<User>> GetUsersWhoHaveSubmittedAppeals();

        Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals();

        public Task<List<User>> GetUsersByRoleType(RoleType roleType);

        public Task<RoleType> GetHighestRoleTypeForUser(Guid userId);

        public Task AddRoleToUser(Guid userID, Role roleToAdd);

        public Task<List<User>> GetAllUsers();
        public Task<bool> ValidateAction(Guid userId, string resource, string action);
        public Task<User?> GetUserByUsername(string username);
        public Task<User?> GetUserById(Guid userId);
        public Task<bool> CreateUser(User user);
        public Task<bool> UpdateUser(User user);
    }
}