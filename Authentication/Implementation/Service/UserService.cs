using System;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.Service;

namespace DrinkDb_Auth.Service
{
    public class UserService
    {
        private readonly UserAdapter userAdapter;
        private readonly AuthenticationService authenticationService;

        private const string UserNotFoundMessage = "User not found";
        private const string NoUserLoggedInMessage = "No user is currently logged in.";
        private const string NullResourceError = "Resource cannot be null or empty.";
        private const string NullActionError = "Action cannot be null or empty.";

        public UserService()
        {
            userAdapter = new UserAdapter();
            authenticationService = new AuthenticationService();
        }

        public User GetUserById(Guid userId)
        {
            return userAdapter.GetUserById(userId) ?? throw new ArgumentException(UserNotFoundMessage, nameof(userId));
        }

        public User GetUserByUsername(string username)
        {
            return userAdapter.GetUserByUsername(username) ?? throw new ArgumentException(UserNotFoundMessage, nameof(username));
        }

        public User GetCurrentUser()
        {
            Guid currentSessionId = App.CurrentSessionId;
            if (currentSessionId == Guid.Empty)
            {
                throw new InvalidOperationException(NoUserLoggedInMessage);
            }
            return authenticationService.GetUser(currentSessionId);
        }

        public bool ValidateAction(Guid userId, string resource, string action)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentException(NullResourceError, nameof(resource));
            }
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentException(NullActionError, nameof(action));
            }
            return userAdapter.ValidateAction(userId, resource, action);
        }

        public void LogoutUser()
        {
            authenticationService.Logout();
        }
    }
}






















