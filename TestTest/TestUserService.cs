using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using DataAccess.Model;
using DrinkDb_Auth.Service;
using DrinkDb_Auth;
using DrinkDb_Auth.Service.Authentication;
using DataAccess.Model.Authentication;

namespace Tests
{
    [TestClass]
    public sealed class TestUserService
    {
        // ─────────────── Mocks ───────────────

        private sealed class MockUserAdapter
        {
            public User? ReturnById { get; set; }
            public User? ReturnByUsername { get; set; }
            public bool ValidateActionResult { get; set; }
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
            User user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "test",
                PasswordHash = "hash",
                TwoFASecret = "2fa"
            };

            MockUserAdapter mockAdapter = new MockUserAdapter { ReturnById = user };

            UserService userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            User result = userService.GetUserById(user.UserId);

            Assert.AreEqual(user, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetUserById_ShouldThrow_WhenUserNotFound()
        {
            MockUserAdapter mockAdapter = new MockUserAdapter { ReturnById = null };

            UserService userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            userService.GetUserById(Guid.NewGuid());
        }

        [TestMethod]
        public void GetUserByUsername_ShouldReturnUser()
        {
            User user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "test",
                PasswordHash = "hash",
                TwoFASecret = "2fa"
            };

            MockUserAdapter mockAdapter = new MockUserAdapter { ReturnByUsername = user };

            UserService userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            User result = userService.GetUserByUsername(user.Username);

            Assert.AreEqual(user, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetUserByUsername_ShouldThrow_WhenUserNotFound()
        {
            MockUserAdapter mockAdapter = new MockUserAdapter { ReturnByUsername = null };

            UserService userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            userService.GetUserByUsername("ghost");
        }

        [TestMethod]
        public void GetCurrentUser_ShouldReturnUser_WhenSessionExists()
        {
            User user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "alpha",
                PasswordHash = "pass",
                TwoFASecret = "2fa"
            };

            App.CurrentSessionId = Guid.NewGuid();

            MockAuthenticationService mockAuth = new MockAuthenticationService(user);
            UserService userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;

            typeof(UserService).GetField("authenticationService", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAuth);

            User result = userService.GetCurrentUser();

            Assert.AreEqual(user, result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetCurrentUser_ShouldThrow_WhenNoSessionExists()
        {
            App.CurrentSessionId = Guid.Empty;

            UserService userService = new UserService();
            userService.GetCurrentUser();
        }

        [TestMethod]
        public void ValidateAction_ShouldReturnTrue_WhenValid()
        {
            var mockAdapter = new MockUserAdapter { ValidateActionResult = true };

            UserService userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("userAdapter", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAdapter);

            Boolean result = userService.ValidateAction(Guid.NewGuid(), "resource", "read");

            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateAction_ShouldThrow_WhenResourceIsEmpty()
        {
            UserService userService = new UserService();
            userService.ValidateAction(Guid.NewGuid(), "", "read");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateAction_ShouldThrow_WhenActionIsEmpty()
        {
            UserService userService = new UserService();
            userService.ValidateAction(Guid.NewGuid(), "resource", "");
        }

        [TestMethod]
        public void LogoutUser_ShouldCallLogout()
        {
            MockAuthenticationService mockAuth = new MockAuthenticationService(new User
            {
                UserId = Guid.NewGuid(),
                Username = "test",
                PasswordHash = "hash",
                TwoFASecret = "2fa"
            });

            UserService userService = (UserService)Activator.CreateInstance(typeof(UserService), true)!;
            typeof(UserService).GetField("authenticationService", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(userService, mockAuth);

            userService.LogoutUser();

            Assert.IsTrue(mockAuth.LogoutCalled);
        }
    }
}
