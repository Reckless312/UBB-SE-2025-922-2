using System;
using DrinkDb_Auth.Service;

namespace DrinkDb_Auth.Model
{
    public class User
    {
        private static readonly UserService UserService = new ();

        public required Guid UserId { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string? TwoFASecret { get; set; }
        public bool ValidateAction(string resource, string action)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentException("Role cannot be null or empty.", nameof(resource));
            }
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentException("Action cannot be null or empty.", nameof(action));
            }
            return UserService.ValidateAction(UserId, resource, action);
        }
    }
}
