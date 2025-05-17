using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DataAccess.Model.Authentication;
using IRepository;
using Repository.AdminDashboard;

namespace DataAccess.AuthProviders
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
    public class BasicAuthenticationProvider : IBasicAuthenticationProvider
    {
        private IUserRepository userRepository;
        public BasicAuthenticationProvider(IUserRepository userRepo)
        {
            userRepository = userRepo;
        }

        public bool Authenticate(string username, string password)
        {
            User? user = userRepository.GetUserByUsername(username).Result ?? throw new UserNotFoundException("User not found");
            byte[] passwordInBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string passwordHash = Convert.ToBase64String(passwordInBytes);
            return user.PasswordHash.SequenceEqual(passwordHash);
        }
    }
}
