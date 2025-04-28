using System;
using CombinedProject.Model;

namespace CombinedProject.Adapter
{
    public interface IUserAdapter
    {
        public bool ValidateAction(Guid userId, string resource, string action);
        public User? GetUserByUsername(string username);
        public User? GetUserById(Guid userId);
        public bool CreateUser(User user);
        public bool UpdateUser(User user);
    }
}
