using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SharedResources.Model.Authentication;
using DrinkDb_Auth.Repository.AdminDashboard;
using SharedResources.Repository.AdminDashboard.Interfaces;

namespace DrinkDb_Auth.AuthProviders
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message)
        {
        }
    }
    public class BasicAuthenticationProvider : IBasicAuthenticationProvider
    {
        private static readonly IUserRepository UserRepository = new UserRepository();

        public bool Authenticate(string username, string password)
        {
            User? user = UserRepository.GetUserByUsername(username) ?? throw new UserNotFoundException("User not found");
            byte[] passwordInBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string passwordHash = Convert.ToBase64String(passwordInBytes);
            return user.PasswordHash.SequenceEqual(passwordHash);
        }
    }
}
