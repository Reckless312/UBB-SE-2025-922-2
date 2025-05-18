using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IUserRepository userRepository;
        public BasicAuthenticationProvider(IUserRepository userRepo)
        {
            userRepository = userRepo;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            System.Diagnostics.Debug.WriteLine($"BasicAuthenticationProvider: Attempting to authenticate {username}");
            User? user = await userRepository.GetUserByUsername(username);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("BasicAuthenticationProvider: User not found");
                throw new UserNotFoundException($"User {username} not found");
            }

            System.Diagnostics.Debug.WriteLine($"BasicAuthenticationProvider: Found user with hash: {user.PasswordHash}");
            string hashedPassword = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
            System.Diagnostics.Debug.WriteLine($"BasicAuthenticationProvider: Computed hash: {hashedPassword}");
            bool isAuthenticated = user.PasswordHash == hashedPassword;
            System.Diagnostics.Debug.WriteLine($"BasicAuthenticationProvider: Authentication result: {isAuthenticated}");
            return isAuthenticated;
        }

        // Keep the sync method for backward compatibility, but make it use the async version
        public bool Authenticate(string username, string password)
        {
            return AuthenticateAsync(username, password).GetAwaiter().GetResult();
        }
    }
}
