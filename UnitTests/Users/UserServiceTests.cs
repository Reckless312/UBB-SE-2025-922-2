﻿// <copyright file="UserServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.Users
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using DataAccess.Model.Authentication;
    using DataAccess.Service;
    using DataAccess.Service.AdminDashboard.Components;
    using DrinkDb_Auth.Service;
    using IRepository;
    using Moq;
    using Xunit;
    using static DrinkDb_Auth.ProxyRepository.AdminDashboard.UserProxyRepository;

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
            this.userService = new UserService(this.mockUserRepository.Object, null);
        }

        /// <summary>
        /// Verifies that the <see cref="UserService"/> constructor initializes correctly when a valid <see cref="IUserRepository"/> is provided.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitialize_WhenUserRepositoryIsValid()
        {
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            //UserService userService = new UserService(mockUserRepository.Object);
            Assert.NotNull(userService);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAllUsers"/> retrieves all users successfully.
        /// </summary>
        [Fact]
        public void GetAllUsers_ShouldReturnAllUsers()
        {
            var mockUserRepository = new Mock<IUserRepository>();
            var users = new List<User>
            {
                new User { UserId = Guid.NewGuid(), EmailAddress = String.Empty, Username = "User One", NumberOfDeletedReviews = 0, AssignedRole = RoleType.User, PasswordHash = String.Empty, TwoFASecret = String.Empty, FullName = "User One" },
                new User {  UserId = Guid.NewGuid(), EmailAddress = String.Empty, Username = "User Two", NumberOfDeletedReviews = 0, AssignedRole = RoleType.Admin, PasswordHash = String.Empty, TwoFASecret = String.Empty, FullName = "User Two" }
            };
            mockUserRepository.Setup(repo => repo.GetAllUsers()).ReturnsAsync(users);

            var userService1 = new UserService(mockUserRepository.Object, null);

            var result = userService1.GetAllUsers().Result;

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("User One", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAllUsers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public async Task GetAllUsers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repo => repo.GetAllUsers()).Throws(new Exception("Repository error"));
            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(async () => await this.userService.GetAllUsers());
            Assert.Equal("Failed to retrieve all users.", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetAllUsers"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetAllUsers_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repo => repo.GetAllUsers()).ReturnsAsync(new List<User>());
            List<User> result = this.userService.GetAllUsers().Result;
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetActiveUsersByRoleType"/> throws an <see cref="ArgumentException"/> when the role type is invalid.
        /// </summary>
        [Fact]
        public async Task GetActiveUsersByRoleType_ShouldThrowArgumentException_WhenRoleTypeIsInvalid()
        {
            var invalidRoleType = (RoleType)(-1);
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await this.userService.GetActiveUsersByRoleType(invalidRoleType));
            Assert.Equal("Role type must be a valid value", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserById"/> retrieves the correct user by ID.
        /// </summary>
        [Fact]
        public void GetUserById_ShouldReturnCorrectUser()
        {
            Guid userId = Guid.NewGuid();

            // Create a concrete User object
            var user = new User
            {
                Username = string.Empty, // Use built-in type alias
                PasswordHash = string.Empty,
                TwoFASecret = string.Empty,
                UserId = userId,
                FullName = "User One"
            };

            // Mock the repository to return the User object
            this.mockUserRepository.Setup(repository => repository.GetUserById(userId)).ReturnsAsync(user);

            // Call the method under test
            var result = this.userService.GetUserById(userId).Result;

            // Assert the result
            Assert.NotNull(result);
            Assert.Equal("User One", result.FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserById"/> throws a <see cref="UserServiceException"/> when the repository returns null.
        /// </summary>
        [Fact]
        public async Task GetUserById_ShouldThrowUserServiceException_WhenRepositoryReturnsNull()
        {
            // Use Guid.NewGuid() instead of the default constructor
            Guid id = Guid.NewGuid();

            // Mock the repository to return null
            this.mockUserRepository
                .Setup(repository => repository.GetUserById(id))
                .ReturnsAsync((User)null);

            // Assert that the exception is thrown
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await this.userService.GetUserById(id));
            Assert.Equal("User not found (Parameter 'userId')", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsers"/> retrieves all banned users successfully.
        /// </summary>
        [Fact]
        public void GetBannedUsers_ShouldReturnBannedUsers()
        {
            List<User> users = new List<User> { new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = new Guid(), FullName = "Banned User" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Banned)).ReturnsAsync(users);
            List<User> result = this.userService.GetBannedUsers().Result;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Banned User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public async Task GetBannedUsers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Banned)).Throws(new Exception("Repository error", new Exception("Inner exception")));

            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetBannedUsers());
            Assert.Equal("Failed to get banned users", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsers"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetBannedUsers_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Banned)).ReturnsAsync(new List<User>());
            List<User> result = this.userService.GetBannedUsers().Result;
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetHighestRoleTypeForUser"/> retrieves the correct highest role type for a user.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldReturnCorrectRoleType()
        {
            this.mockUserRepository.Setup(repository => repository.GetRoleTypeForUser(new Guid())).ReturnsAsync(RoleType.Admin);
            RoleType result = this.userService.GetHighestRoleTypeForUser(new Guid()).Result;
            Assert.Equal(RoleType.Admin, result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetHighestRoleTypeForUser"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public async Task GetHighestRoleTypeForUser_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            var id = new Guid();
            this.mockUserRepository.Setup(repo => repo.GetRoleTypeForUser(id)).Throws(new Exception("Repository error", new Exception("Inner exception")));

            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetHighestRoleTypeForUser(id));
            Assert.Equal("Failed to retrieve the highest role type for user with ID " + id.ToString() + ".", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetHighestRoleTypeForUser"/> returns the default role type when the repository returns a default value.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldReturnDefaultRole_WhenRepositoryReturnsDefault()
        {
            this.mockUserRepository.Setup(repository => repository.GetRoleTypeForUser(new Guid())).ReturnsAsync(RoleType.Banned);
            RoleType result = this.userService.GetHighestRoleTypeForUser(new Guid()).Result;
            Assert.Equal(RoleType.Banned, result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUsersByRoleType"/> retrieves users by role type successfully.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = new Guid(), FullName = "User One" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).ReturnsAsync(users);
            List<User> result = this.userService.GetUsersByRoleType(RoleType.User).Result;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("User One", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUsersByRoleType"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public async Task GetUsersByRoleType_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Throws(new Exception("Repository error", new Exception("Inner exception")));

            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetUsersByRoleType(RoleType.User));
            Assert.Equal("Failed to retrieve users by role type 'User'.", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUsersByRoleType"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).ReturnsAsync(new List<User>());
            List<User> result = this.userService.GetUsersByRoleType(RoleType.User).Result;
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetRegularUsers"/> retrieves all regular users successfully.
        /// </summary>
        [Fact]
        public void GetRegularUsers_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = new Guid(), FullName = "Regular User" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).ReturnsAsync(users);
            List<User> result = this.userService.GetRegularUsers().Result;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Regular User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetRegularUsers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public async Task GetRegularUsers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.User)).Throws(new Exception("Repository error", new Exception("Inner exception")));
            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetRegularUsers());
            Assert.Equal("Failed to retrieve regular users.", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetManagers"/> retrieves all manager users successfully.
        /// </summary>
        [Fact]
        public void GetManagers_ShouldReturnCorrectUsers()
        {
            var users = new List<User> { new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = new Guid(), FullName = "Manager User" } };
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Manager)).ReturnsAsync(users);
            List<User> result = this.userService.GetManagers().Result;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Manager User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetManagers"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public async Task GetManagers_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repository => repository.GetUsersByRoleType(RoleType.Manager)).Throws(new Exception("Repository error", new Exception("Inner exception")));
            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetManagers());
            Assert.Equal("Failed to retrieve manager users.", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsersWhoHaveSubmittedAppeals"/> retrieves all banned users who have submitted appeals successfully.
        /// </summary>
        [Fact]
        public void GetBannedUsersWhoHaveSubmittedAppeals_ShouldReturnCorrectUsers()
        {
            List<User> users = new List<User> { new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = new Guid(), FullName = "Banned User", HasSubmittedAppeal = true } };
            this.mockUserRepository.Setup(repo => repo.GetBannedUsersWhoHaveSubmittedAppeals()).ReturnsAsync(users);
            List<User> result = this.userService.GetBannedUsersWhoHaveSubmittedAppeals().Result;
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Banned User", result[0].FullName);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsersWhoHaveSubmittedAppeals"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public async Task GetBannedUsersWhoHaveSubmittedAppeals_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            this.mockUserRepository.Setup(repo => repo.GetBannedUsersWhoHaveSubmittedAppeals()).Throws(new Exception("Repository error", new Exception("Inner exception")));
            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetBannedUsersWhoHaveSubmittedAppeals());
            Assert.Equal("Failed to retrieve banned users who have submitted appeals.", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetBannedUsersWhoHaveSubmittedAppeals"/> returns an empty list when the repository returns no users.
        /// </summary>
        [Fact]
        public void GetBannedUsersWhoHaveSubmittedAppeals_ShouldReturnEmptyList_WhenRepositoryReturnsNoUsers()
        {
            this.mockUserRepository.Setup(repository => repository.GetBannedUsersWhoHaveSubmittedAppeals()).ReturnsAsync(new List<User>());
            List<User> result = this.userService.GetBannedUsersWhoHaveSubmittedAppeals().Result;
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserFullNameById"/> retrieves the correct full name of a user by ID.
        /// </summary>
        [Fact]
        public void GetUserFullNameById_ShouldReturnCorrectFullName()
        {
            // Use Guid.NewGuid() instead of the default constructor
            Guid userId = Guid.NewGuid();

            // Create a concrete User object
            var user = new User
            {
                Username = "User One",
                PasswordHash = string.Empty,
                TwoFASecret = string.Empty,
                UserId = userId,
                FullName = "User One"
            };

            // Mock the repository to return the User object
            this.mockUserRepository
                .Setup(repository => repository.GetUserById(userId))
                .ReturnsAsync(user);

            // Call the method under test
            string result = this.userService.GetUserFullNameById(userId).Result;

            // Assert the result
            Assert.Equal("User One", result);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserFullNameById"/> throws a <see cref="UserServiceException"/> when the repository throws an exception.
        /// </summary>
        [Fact]
        public async Task GetUserFullNameById_ShouldThrowUserServiceException_WhenRepositoryThrowsException()
        {
            var id = new Guid();
            this.mockUserRepository.Setup(repository => repository.GetUserById(id)).Throws(new Exception("Repository error", new Exception("Inner exception")));
            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetUserFullNameById(id));
            Assert.Equal("Failed to retrieve the full name of the user with ID " + id.ToString() + ".", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.GetUserFullNameById"/> throws a <see cref="UserServiceException"/> when the repository returns null.
        /// </summary>
        [Fact]
        public async Task GetUserFullNameById_ShouldThrowUserServiceException_WhenRepositoryReturnsNull()
        {
            var id = new Guid();
            this.mockUserRepository.Setup(repository => repository.GetUserById(id)).ReturnsAsync((User)null);
            UserServiceException exception = await Assert.ThrowsAsync<UserServiceException>(() => this.userService.GetUserFullNameById(id));
            Assert.Equal("Failed to retrieve the full name of the user with ID " + id.ToString() + ".", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> does nothing when the user does not exist.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldDoNothing_WhenUserDoesNotExist()
        {
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).ReturnsAsync((User)null);

            this.userService.UpdateUserRole(new Guid(), RoleType.Banned);

            this.mockUserRepository.Verify(repository => repository.ChangeRoleToUser(It.IsAny<Guid>(), It.IsAny<Role>()), Times.Never);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> does not add the banned role when the user already has it.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldNotAddBannedRole_WhenUserAlreadyHasBannedRole()
        {
            User user = new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = new Guid(), AssignedRole = RoleType.Banned };
            this.mockUserRepository.Setup(repository => repository.GetUserById(new Guid())).ReturnsAsync(user);
            this.userService.UpdateUserRole(new Guid(), RoleType.Banned);
            Assert.Equal(RoleType.Banned, user.AssignedRole);
            this.mockUserRepository.Verify(repository => repository.ChangeRoleToUser(It.IsAny<Guid>(), It.IsAny<Role>()), Times.Never);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> sets the role to banned when the user does not already have it.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldSetRoleToBanned_WhenUserDoesNotHaveBannedRole()
        {
            Guid userId = Guid.NewGuid();
            User user = new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = userId, AssignedRole = RoleType.User };
            this.mockUserRepository.Setup(repository => repository.GetUserById(userId)).ReturnsAsync(user);
            this.userService.UpdateUserRole(userId, RoleType.Banned);
            this.mockUserRepository.Verify(repository => repository.UpdateUser(It.Is<User>(user => user.UserId == userId && user.AssignedRole == RoleType.Banned)), Times.Once);
        }

        /// <summary>
        /// Verifies that <see cref="UserService.UpdateUserRole"/> sets the role to user when the role type is user.
        /// </summary>
        [Fact]
        public void UpdateUserRole_ShouldSetRoleToUser_WhenRoleTypeIsUser()
        {
            Guid userId = Guid.NewGuid();
            User user = new User { Username = String.Empty, PasswordHash = String.Empty, TwoFASecret = String.Empty, UserId = userId, AssignedRole = RoleType.Banned };
            this.mockUserRepository.Setup(repository => repository.GetUserById(userId)).ReturnsAsync(user);
            this.userService.UpdateUserRole(userId, RoleType.User);
            this.mockUserRepository.Verify(repo => repo.UpdateUser(It.Is<User>(user => user.UserId == userId && user.AssignedRole == RoleType.User)), Times.Once);
        }


    }
}