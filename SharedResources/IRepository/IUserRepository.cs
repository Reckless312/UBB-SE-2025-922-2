namespace IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;

    public interface IUserRepository
    {
        public Task<List<User>> GetUsersWhoHaveSubmittedAppeals();

        Task<List<User>> GetBannedUsersWhoHaveSubmittedAppeals();

        public Task<List<User>> GetUsersByRoleType(RoleType roleType);

        public Task<RoleType?> GetRoleTypeForUser(Guid userId);

        public Task ChangeRoleToUser(Guid userID, Role roleToAdd);

        public Task<List<User>> GetAllUsers();

        public Task<User?> GetUserByUsername(string username);

        public Task<User?> GetUserById(Guid userId);

        public Task<bool> CreateUser(User user);

        public Task<bool> UpdateUser(User user);
    }
}