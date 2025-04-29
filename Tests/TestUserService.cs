using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using DrinkDb_Auth.Model;
using DrinkDb_Auth.Service;
using DrinkDb_Auth.Adapter;
using DrinkDb_Auth;

namespace Tests
{
    [TestClass]
    public sealed class TestUserService
    {
        // ─────────────── Mocks ───────────────

        private sealed class MockUserAdapter : UserAdapter
        {
            public User? ReturnById { get; set; }
            public User? ReturnByUsername { get; set; }
            public bool ValidateActionResult { get; set; }

            public override User? GetUserById(Guid userId) => ReturnById;
            public override User? GetUserByUsername(string username) => ReturnByUsername;
            public override bool ValidateAction(Guid userId, string resource, string action) => ValidateActionResult;
        }

        private sealed class MockAuthenticationService : AuthenticationService
        {
            private readonly User _user;
            public bool LogoutCalled { get; private set; }

            public MockAuthenticationService(User user)
            {
                _user = user;
                LogoutCalled = false;
            }

            public override User GetUser(Guid sessionId) => _user;

            public override void Logout() => LogoutCalled = true;
        }

        // ─────────────── Test Cases ───────────────

        [TestMethod]
        public void GetUserById_ShouldReturnUser()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "test",
                PasswordHash = "hash",
                TwoFASecret = "2fa"
            };

            var mockAdapter = new MockUserAdapter { ReturnById = user };

            var userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            var result = userService.GetUserById(user.UserId);

            Assert.AreEqual(user, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetUserById_ShouldThrow_WhenUserNotFound()
        {
            var mockAdapter = new MockUserAdapter { ReturnById = null };

            var userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            userService.GetUserById(Guid.NewGuid());
        }

        [TestMethod]
        public void GetUserByUsername_ShouldReturnUser()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "test",
                PasswordHash = "hash",
                TwoFASecret = "2fa"
            };

            var mockAdapter = new MockUserAdapter { ReturnByUsername = user };

            var userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            var result = userService.GetUserByUsername(user.Username);

            Assert.AreEqual(user, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetUserByUsername_ShouldThrow_WhenUserNotFound()
        {
            var mockAdapter = new MockUserAdapter { ReturnByUsername = null };

            var userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            userService.GetUserByUsername("ghost");
        }

        [TestMethod]
        public void GetCurrentUser_ShouldReturnUser_WhenSessionExists()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "alpha",
                PasswordHash = "pass",
                TwoFASecret = "2fa"
            };

            App.CurrentSessionId = Guid.NewGuid();

            var mockAuth = new MockAuthenticationService(user);
            var userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;

            typeof(UserService).GetField("authenticationService", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAuth);

            var result = userService.GetCurrentUser();

            Assert.AreEqual(user, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetCurrentUser_ShouldThrow_WhenNoSessionExists()
        {
            App.CurrentSessionId = Guid.Empty;

            var userService = new UserService();
            userService.GetCurrentUser();
        }

        [TestMethod]
        public void ValidateAction_ShouldReturnTrue_WhenValid()
        {
            var mockAdapter = new MockUserAdapter { ValidateActionResult = true };

            var userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            var result = userService.ValidateAction(Guid.NewGuid(), "resource", "read");

            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateAction_ShouldThrow_WhenResourceIsEmpty()
        {
            var userService = new UserService();
            userService.ValidateAction(Guid.NewGuid(), "", "read");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateAction_ShouldThrow_WhenActionIsEmpty()
        {
            var userService = new UserService();
            userService.ValidateAction(Guid.NewGuid(), "resource", "");
        }

        [TestMethod]
        public void LogoutUser_ShouldCallLogout()
        {
            var mockAuth = new MockAuthenticationService(new User
            {
                UserId = Guid.NewGuid(),
                Username = "test",
                PasswordHash = "hash",
                TwoFASecret = "2fa"
            });

            var userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("authenticationService", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAuth);

            userService.LogoutUser();

            Assert.IsTrue(mockAuth.LogoutCalled);
        }
    }
}
