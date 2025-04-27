using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.Model;

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
        private static readonly IUserAdapter UserDatabaseAdapter = new UserAdapter();

        public bool Authenticate(string username, string password)
        {
            User? user = UserDatabaseAdapter.GetUserByUsername(username) ?? throw new UserNotFoundException("User not found");
            byte[] passwordInBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string passwordHash = Convert.ToBase64String(passwordInBytes);
            return user.PasswordHash.SequenceEqual(passwordHash);
        }
    }
}
