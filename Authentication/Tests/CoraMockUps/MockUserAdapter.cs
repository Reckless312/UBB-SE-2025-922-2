using DrinkDb_Auth.Model;
using System;

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
    }
}
