// <copyright file="UserServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.Users
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DrinkDb_Auth.Service;
    using DrinkDb_Auth.Service.AdminDashboard.Components;
    using IRepository;
    using Moq;
    using Xunit;
    using static DrinkDb_Auth.Repository.AdminDashboard.UserRepository;

    /// <summary>
    /// Unit tests for the <see cref="UserService"/> class.
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly UserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
        /// Initializes a mock of the <see cref="UserRepository"/> and a new instance of the <see cref="UserServiceTests"/> class.
        /// </summary>
        public UserServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.userService = new UserService(this.mockUserRepository.Object);
        }

        /// <summary>
        /// Verifies that the <see cref="UserService"/> constructor initializes correctly when a valid <see cref="IUserRepository"/> is provided.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitialize_WhenUserRepositoryIsValid()
        {
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            UserService userService = new UserService(mockUserRepository.Object);
            Assert.NotNull(userService);
        }

        /// <summary>
        /// Verifies that the <see cref="UserService"/> constructor throws an <see cref="ArgumentNullException"/> when the <see cref="IUserRepository"/> is null.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenUserRepositoryIsNull()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new UserService(null));
            Assert.Equal("Value cannot be null. (Parameter 'userRepository')", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAllUsers"/> retrieves all users successfully.
        /// </summary>
        [Fact]
        public void GetAllUsers_ShouldReturnAllUsers()
        {
            List<User> users = new List<User> { new User { Username =  "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "User One" }, new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "User Two" } };
            this.mockUserRepository.Setup(repo => repo.GetAllUsers()).Returns(users);
            List<User> result = this.userService.GetAllUsers();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("User One", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAllUsers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetAllUsers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repo => repo.GetAllUsers()).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetAllUsers());
            Assert.Equal("Failed to retrieve all users.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAllUsers"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetAllUsers_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repo => repo.GetAllUsers()).Returns(new List<User>());
            List<User> result = this.userService.GetAllUsers();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetActiveUsersByRoleType"/> retrieves active users by role type successfully.
        /// </summary>
        [Fact]
        public void GetActiveUsersByRoleType_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = "", PasswordHash = "", TwoFASecret = "" ,UserId = new Guid(), FullName = "Active User" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Returns(users);
            List<User> result = this.userService.GetActiveUsersByRoleType(RoleType.User);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Active User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetActiveUsersByRoleType"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetActiveUsersByRoleType_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetActiveUsersByRoleType(RoleType.User));
            Assert.Equal("Failed to get active users", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetActiveUsersByRoleType"/> throws an <see cref="ArgumentException"/> when the role type is invalid.
        /// </summary>
        [Fact]
        public void GetActiveUsersByRoleType_ShouldThrowArgumentException_WhenRoleTypeIsInvalid()
        {
            var exception = Assert.Throws<ArgumentException>(() => this.userService.GetActiveUsersByRoleType(0));
            Assert.Equal("Permission ID must be positive", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserById"/> retrieves the correct user by ID.
        /// </summary>
        [Fact]
        public void GetUserById_ShouldReturnCorrectUser()
        {
            User user = new User {Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "User One" };
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid()).Returns(user));
            User result = userService.GetUserById(new Guid());
            Assert.NotNull(result);
            Assert.Equal("User One", result.FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserById"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetUserById_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetUserById(new Guid()));
            Assert.Equal("Failed to retrieve user with ID 1.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserById"/> throws a <see cref="UserServiceException"/> when the repository returns null.
        /// </summary>
        [Fact]
        public void GetUserById_ShouldThrowUserServiceException_WhenRepositoryReturnsNull()
        {
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Returns((User)null);
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetUserById(new Guid()));
            Assert.Equal("Failed to retrieve user with ID 1.", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsers"/> retrieves all banned users successfully.
        /// </summary>
        [Fact]
        public void GetBannedUsers_ShouldReturnBannedUsers()
        {
            List<User> users = new List<User> { new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "Banned User" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Banned)).Returns(users);
            List<User> result = this.userService.GetBannedUsers();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Banned User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetBannedUsers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Banned)).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));

            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetBannedUsers());
            Assert.Equal("Failed to get banned users", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsers"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetBannedUsers_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Banned)).Returns(new List<User>());
            List<User> result = this.userService.GetBannedUsers();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetHighestRoleTypeForUser"/> retrieves the correct highest role type for a user.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldReturnCorrectRoleType()
        {
            this.mockUserRepository.Setup(repository => repository.GetHighestRoleTypeForUser(new Guid())).Returns(RoleType.Admin);
            RoleType result = this.userService.GetHighestRoleTypeForUser(new Guid());
            Assert.Equal(RoleType.Admin, result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetHighestRoleTypeForUser"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repo => repo.GetHighestRoleTypeForUser(new Guid())).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));

            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetHighestRoleTypeForUser(new Guid()));
            Assert.Equal("Failed to retrieve the highest role type for user with ID 1.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetHighestRoleTypeForUser"/> returns the default role type when the repository returns a default value.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldReturnDefaultRole_WhenRepositoryReturnsDefault()
        {
            this.mockUserRepository.Setup(repository => repository.GetHighestRoleTypeForUser(new Guid())).Returns(RoleType.Banned);
            RoleType result = this.userService.GetHighestRoleTypeForUser(new Guid());
            Assert.Equal(RoleType.Banned, result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUsersByRoleType"/> retrieves users by role type successfully.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "User One" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Returns(users);
            List<User> result = this.userService.GetUsersByRoleType(RoleType.User);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("User One", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUsersByRoleType"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));

            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetUsersByRoleType(RoleType.User));
            Assert.Equal("Failed to retrieve users by role type 'User'.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUsersByRoleType"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Returns(new List<User>());
            List<User> result = this.userService.GetUsersByRoleType(RoleType.User);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAdminUsers"/> retrieves all admin users successfully.
        /// </summary>
        [Fact]
        public void GetAdminUsers_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "Admin One" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Admin)).Returns(users);
            List<User> result = this.userService.GetAdminUsers();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Admin One", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAdminUsers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetAdminUsers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Admin)).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetAdminUsers());
            Assert.Equal("Failed to retrieve admin users.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetRegularUsers"/> retrieves all regular users successfully.
        /// </summary>
        [Fact]
        public void GetRegularUsers_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "Regular User" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Returns(users);
            List<User> result = this.userService.GetRegularUsers();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Regular User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetRegularUsers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetRegularUsers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetRegularUsers());
            Assert.Equal("Failed to retrieve regular users.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetManagers"/> retrieves all manager users successfully.
        /// </summary>
        [Fact]
        public void GetManagers_ShouldReturnCorrectUsers()
        {
            var users = new List<User> { new User {Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "Manager User" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Manager)).Returns(users);
            List<User> result = this.userService.GetManagers();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Manager User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetManagers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetManagers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Manager)).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetManagers());
            Assert.Equal("Failed to retrieve manager users.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsersWhoHaveSubmittedAppeals"/> retrieves all banned users who have submitted appeals successfully.
        /// </summary>
        [Fact]
        public void GetBannedUsersWhoHaveSubmittedAppeals_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "Banned User", HasSubmittedAppeal = true } };
            this.mockUserRepository.Setup(repo => repo.GetBannedUsersWhoHaveSubmittedAppeals()).Returns(users);
            List<User> result = this.userService.GetBannedUsersWhoHaveSubmittedAppeals();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Banned User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsersWhoHaveSubmittedAppeals"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetBannedUsersWhoHaveSubmittedAppeals_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repo => repo.GetBannedUsersWhoHaveSubmittedAppeals()).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetBannedUsersWhoHaveSubmittedAppeals());
            Assert.Equal("Failed to retrieve banned users who have submitted appeals.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsersWhoHaveSubmittedAppeals"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetBannedUsersWhoHaveSubmittedAppeals_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repository => repository.GetBannedUsersWhoHaveSubmittedAppeals()).Returns(new List<User>());
            List<User> result = this.userService.GetBannedUsersWhoHaveSubmittedAppeals();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserFullNameById"/> retrieves the correct full name of a user by ID.
        /// </summary>
        [Fact]
        public void GetUserFullNameById_ShouldReturnCorrectFullName()
        {
            User user = new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), FullName = "User One" };
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Returns(user);
            string result = this.userService.GetUserFullNameById(new Guid());
            Assert.Equal("User One", result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserFullNameById"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void GetUserFullNameById_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetUserFullNameById(new Guid()));
            Assert.Equal("Failed to retrieve the full name of the user with ID 1.", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserFullNameById"/> throws a <see cref="UserServiceException"/> when the repository returns null.
        /// </summary>
        [Fact]
        public void GetUserFullNameById_ShouldThrowUserServiceException_WhenRepositoryReturnsNull()
        {
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Returns((User)null);
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.GetUserFullNameById(new Guid()));
            Assert.Equal("Failed to retrieve the full name of the user with ID 1.", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> does nothing when the user does not exist.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldDoNothing_WhenUserDoesNotExist()
        {
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Returns((User)null);

            this.userService.UpdateUserRole(new Guid(), RoleType.Banned);

            this.mockUserRepository.Verify(repository => repository.AddRoleToUser(It.IsAny<int>(), It.IsAny<Role>()), Times.Never);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> does not add the banned role when the user already has it.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldNotAddBannedRole_WhenUserAlreadyHasBannedRole()
        {
            User user = new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), AssignedRoles = new List<Role> { new Role(RoleType.Banned, "Banned") } };
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Returns(user);
            this.userService.UpdateUserRole(new Guid(), RoleType.Banned);
            Assert.Single(user.AssignedRoles);
            Assert.Equal(RoleType.Banned, user.AssignedRoles[0].RoleType);
            this.mockUserRepository.Verify(repository => repository.AddRoleToUser(It.IsAny<int>(), It.IsAny<Role>()), Times.Never);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> sets the role to banned when the user does not already have it.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldSetRoleToBanned_WhenUserDoesNotHaveBannedRole()
        {
            User user = new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), AssignedRoles = new List<Role> { new Role(RoleType.User, "User") } };
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Returns(user);
            this.userService.UpdateUserRole(new Guid(), RoleType.Banned);
            this.mockUserRepository.Verify(repository => repository.AddRoleToUser(new Guid(), It.Is<Role>(r => r.RoleType == RoleType.Banned && r.RoleName == "Banned")), Times.Once);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> sets the role to user when the role type is user.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldSetRoleToUser_WhenRoleTypeIsUser()
        {
            User user = new User { Username = "", PasswordHash = "", TwoFASecret = "", UserId = new Guid(), AssignedRoles = new List<Role> { new Role(RoleType.Banned, "Banned") } };
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).Returns(user);
            this.userService.UpdateUserRole(new Guid(), RoleType.User);
            this.mockUserRepository.Verify(repo => repo.AddRoleToUser(new Guid(), It.Is<Role>(r => r.RoleType == RoleType.User && r.RoleName == "User")), Times.Once);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repo => repo.GetUserById(new Guid())).Throws(new RepositoryException("Repository error", new Exception("Inner exception")));
            UserServiceException exception = Assert.Throws<UserServiceException>(() => this.userService.UpdateUserRole(new Guid(), RoleType.Banned));
            Assert.Equal("Failed to update user role", exception.Message);
            Assert.IsType<RepositoryException>(exception.InnerException);
        }
    }
}
