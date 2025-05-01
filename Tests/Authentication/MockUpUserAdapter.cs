using DataAccess.Model.AdminDashboard;
using DataAccess.Model.Authentication;
using System;
using System.Collections.Generic;

namespace Tests.Authentication
{
    internal class MockUpUserAdapter : IMockUpUserAdapter
    {
        public MockUpUserAdapter() { }
        public bool UpdatedUser { get; set; } = false;
        User IMockUpUserAdapter.User { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void AddRoleToUser(Guid userID, Role roleToAdd)
        {
            throw new NotImplementedException();
        }

        public void AddRoleToUser(int v, Role role)
        {
            throw new NotImplementedException();
        }

        public bool CreateUser(User user)
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

        public RoleType GetHighestRoleTypeForUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void GetHighestRoleTypeForUser(int v)
        {
            throw new NotImplementedException();
        }

        public User? GetUserById(Guid userId)
        {
            throw new NotImplementedException();
        }

        public User? GetUserByUsername(string username)
        {
            throw new NotImplementedException();
        }

        public List<User> GetUsersByRoleType(RoleType roleType)
        {
            throw new NotImplementedException();
        }

        public List<User> GetUsersWhoHaveSubmittedAppeals()
        {
            throw new NotImplementedException();
        }

        public bool UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public bool ValidateAction(Guid userId, string resource, string action)
        {
            throw new NotImplementedException();
        }
    }
}