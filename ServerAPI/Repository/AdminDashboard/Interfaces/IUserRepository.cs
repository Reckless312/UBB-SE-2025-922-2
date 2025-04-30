// <copyright file="IUserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Repository.AdminDashboard.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Model.AdminDashboard;
    using Model.Authentication;

    public interface IUserRepository
    {
        // public void UpdateRole(int userID, int permissionID);
        public List<User> GetUsersWhoHaveSubmittedAppeals();

        List<User> GetBannedUsersWhoHaveSubmittedAppeals();

        public List<User> GetUsersByRoleType(RoleType roleType);

        public RoleType GetHighestRoleTypeForUser(Guid userId);

        public void AddRoleToUser(Guid userID, Role roleToAdd);

        public List<User> GetAllUsers();

        public bool ValidateAction(Guid userId, string resource, string action);
        public User? GetUserByUsername(string username);
        public User? GetUserById(Guid userId);
        public bool CreateUser(User user);
        public bool UpdateUser(User user);
    }
}