using DataAccess.Model;
using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using IRepository;
using System;
using System.Collections.Generic;

namespace Tests.CoraMockUps
{
    public class MockUpUserAdapter : IMockUpUserAdapter
    {
        public bool CreatedUser { get; set; }
        public User? User { get; set; }
        public bool UpdatedUser { get; set; }

        public bool CreateUser(User user)
        {
            return CreatedUser;
        }

        public User? GetUserById(Guid userId)
        {
            return User;
        }

        public User? GetUserByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public bool UpdateUser(User user)
        {
            return UpdatedUser;
        }

        public bool ValidateAction(Guid userId, string resource, string action)
        {
            throw new NotImplementedException();
        }

        public List<User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public List<User> GetBannedUsersWhoHaveSubmittedAppeals()
        {
            throw new NotImplementedException();
        }

        public List<User> GetUsersByRoleType(RoleType roleType)
        {
            throw new NotImplementedException();
        }

        public RoleType GetHighestRoleTypeForUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void AddRoleToUser(Guid userID, Role roleToAdd)
        {
            throw new NotImplementedException();
        }

        public List<User> GetUsersWhoHaveSubmittedAppeals()
        {
            throw new NotImplementedException();
        }

        public void GetHighestRoleTypeForUser(int v)
        {
            throw new NotImplementedException();
        }

        public void AddRoleToUser(int v, Role role)
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IUserRepository.GetUsersWhoHaveSubmittedAppeals()
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IUserRepository.GetBannedUsersWhoHaveSubmittedAppeals()
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IUserRepository.GetUsersByRoleType(RoleType roleType)
        {
            throw new NotImplementedException();
        }

        public Task<RoleType> GetRoleTypeForUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task ChangeRoleToUser(Guid userID, Role roleToAdd)
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IUserRepository.GetAllUsers()
        {
            return Task.FromResult(GetAllUsers());
        }

        Task<bool> IUserRepository.ValidateAction(Guid userId, string resource, string action)
        {
            return Task.FromResult(ValidateAction(userId, resource, action));
        }

        Task<User?> IUserRepository.GetUserByUsername(string username)
        {
            return Task.FromResult(GetUserByUsername(username));
        }

        Task<User?> IUserRepository.GetUserById(Guid userId)
        {
            return Task.FromResult(GetUserById(userId));
        }

        Task<bool> IUserRepository.CreateUser(User user)
        {
            return Task.FromResult(CreateUser(user));
        }

        Task<bool> IUserRepository.UpdateUser(User user)
        {
            return Task.FromResult(UpdateUser(user));
        }
    }
}