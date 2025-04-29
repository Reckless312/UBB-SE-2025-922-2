namespace UnitTests.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using App1.Models;
    using App1.Repositories;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="UserRepository"/> class.
    /// </summary>
    public class UserRepositoryTests
    {
        private readonly UserRepository userRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepositoryTests"/> class.
        /// </summary>
        public UserRepositoryTests()
        {
            this.userRepo = new UserRepository();
        }

        /// <summary>
        /// Verifies that the <see cref="UserRepository"/> constructor initializes the user list correctly.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeUsersListCorrectly()
        {
            UserRepository userRepo = new UserRepository();
            List<User> users = userRepo.GetAllUsers();
            Assert.NotNull(users);
            Assert.Equal(3, users.Count);

            User user1 = users.FirstOrDefault(u => u.UserId == 1);
            Assert.NotNull(user1);
            Assert.Equal("bianca.georgiana.cirnu@gmail.com", user1.EmailAddress);
            Assert.Equal("Bianca Georgiana Cirnu", user1.FullName);
            Assert.Equal(2, user1.NumberOfDeletedReviews);
            Assert.True(user1.HasSubmittedAppeal);
            Assert.Single(user1.AssignedRoles);
            Assert.Contains(user1.AssignedRoles, role => role.RoleType == RoleType.User);

            User user2 = users.FirstOrDefault(u => u.UserId == 3);
            Assert.NotNull(user2);
            Assert.Equal("admin.one@example.com", user2.EmailAddress);
            Assert.Equal("Admin One", user2.FullName);
            Assert.Equal(0, user2.NumberOfDeletedReviews);
            Assert.False(user2.HasSubmittedAppeal);
            Assert.Equal(2, user2.AssignedRoles.Count);
            Assert.Contains(user2.AssignedRoles, role => role.RoleType == RoleType.Admin);
            Assert.Contains(user2.AssignedRoles, role => role.RoleType == RoleType.User);

            User user3 = users.FirstOrDefault(u => u.UserId == 5);
            Assert.NotNull(user3);
            Assert.Equal("admin.two@example.com", user3.EmailAddress);
            Assert.Equal("Admin Two", user3.FullName);
            Assert.Equal(0, user3.NumberOfDeletedReviews);
            Assert.False(user3.HasSubmittedAppeal);
            Assert.Equal(2, user3.AssignedRoles.Count);
            Assert.Contains(user3.AssignedRoles, role => role.RoleType == RoleType.Admin);
            Assert.Contains(user3.AssignedRoles, role => role.RoleType == RoleType.User);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersWhoHaveSubmittedAppeals"/> returns the correct users.
        /// </summary>
        [Fact]
        public void GetUsersWhoHaveSubmittedAppeals_ShouldReturnCorrectUsers()
        {
            List<User> users = this.userRepo.GetUsersWhoHaveSubmittedAppeals();

            Assert.NotNull(users);
            Assert.Single(users);
            Assert.True(users[0].HasSubmittedAppeal);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersWhoHaveSubmittedAppeals"/> returns an empty list when no users have submitted appeals.
        /// </summary>
        [Fact]
        public void GetUsersWhoHaveSubmittedAppeals_ShouldReturnEmptyList_WhenNoUsersHaveSubmittedAppeals()
        {
            UserRepository userRepo = new UserRepository();
            userRepo.GetAllUsers().ForEach(user => user.HasSubmittedAppeal = false);
            List<User> users = userRepo.GetUsersWhoHaveSubmittedAppeals();
            Assert.NotNull(users);
            Assert.Empty(users);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersWhoHaveSubmittedAppeals"/> throws a <see cref="UserRepository.RepositoryException"/> when an exception occurs.
        /// </summary>
        [Fact]
        public void GetUsersWhoHaveSubmittedAppeals_ShouldThrowRepositoryException_WhenExceptionOccurs()
        {
            UserRepository userRepo = new UserRepository();
            System.Reflection.FieldInfo usersListField = typeof(UserRepository).GetField("_usersList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            usersListField.SetValue(userRepo, null);
            UserRepository.RepositoryException repoException = Assert.Throws<UserRepository.RepositoryException>(() => userRepo.GetUsersWhoHaveSubmittedAppeals());
            Assert.Equal("Failed to retrieve users who have submitted appeals.", repoException.Message);
            Assert.IsType<NullReferenceException>(repoException.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersByRoleType"/> returns the correct users for a given role type.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldReturnCorrectUsers()
        {
            List<User> adminUsers = this.userRepo.GetUsersByRoleType(RoleType.Admin);
            Assert.NotNull(adminUsers);
            Assert.Equal(2, adminUsers.Count);
            Assert.All(adminUsers, user => Assert.Contains(user.AssignedRoles, role => role.RoleType == RoleType.Admin));
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersByRoleType"/> returns an empty list when no users have the specified role type.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldReturnEmptyList_WhenNoUsersHaveRole()
        {
            UserRepository userRepo = new UserRepository();
            List<User> bannedUsers = userRepo.GetUsersByRoleType(RoleType.Banned);
            Assert.NotNull(bannedUsers);
            Assert.Empty(bannedUsers);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersByRoleType"/> throws a <see cref="UserRepository.RepositoryException"/> when an exception occurs.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldThrowRepositoryException_WhenExceptionOccurs()
        {
            UserRepository userRepo = new UserRepository();
            System.Reflection.FieldInfo usersListField = typeof(UserRepository).GetField("_usersList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            usersListField.SetValue(userRepo, null);
            UserRepository.RepositoryException repoException = Assert.Throws<UserRepository.RepositoryException>(() => userRepo.GetUsersByRoleType(RoleType.Admin));
            Assert.Equal("Failed to retrieve users with role type 'Admin'.", repoException.Message);
            Assert.IsType<NullReferenceException>(repoException.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersByRoleType"/> skips when role is not assigned.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldSkipUsers_WhenAssignedRolesIsNull()
        {
            UserRepository userRepository = new UserRepository();
            User user = new User { UserId = 6, AssignedRoles = null };
            System.Reflection.FieldInfo usersListField = typeof(UserRepository).GetField("_usersList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            List<User> usersList = (List<User>)usersListField.GetValue(userRepository);
            usersList.Add(user);
            List<User> result = userRepository.GetUsersByRoleType(RoleType.Admin);
            Assert.NotNull(result);
            Assert.DoesNotContain(result, u => u.UserId == 6);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUsersByRoleType"/> skips when the role type does not match.
        /// </summary>
        [Fact]
        public void GetUsersByRoleType_ShouldSkipUsers_WhenRoleTypeDoesNotMatch()
        {
            UserRepository userRepo = new UserRepository();
            User user = new User
            {
                UserId = 7,
                AssignedRoles = new List<Role> { new Role(RoleType.User, "User") },
            };

            System.Reflection.FieldInfo usersListField = typeof(UserRepository).GetField("_usersList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            List<User> usersList = (List<User>)usersListField.GetValue(userRepo);
            usersList.Add(user);

            List<User> result = userRepo.GetUsersByRoleType(RoleType.Admin);

            Assert.NotNull(result);
            Assert.DoesNotContain(result, u => u.UserId == 7);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetHighestRoleTypeForUser"/> returns the correct highest role type for a user.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldReturnCorrectRoleType()
        {
            RoleType highestRole = this.userRepo.GetHighestRoleTypeForUser(5);
            Assert.Equal(RoleType.Admin, highestRole);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetHighestRoleTypeForUser"/> throws a <see cref="UserRepository.RepositoryException"/> when the user has no roles.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldThrowException_WhenUserHasNoRoles()
        {
            UserRepository userRepo = new UserRepository();
            User user = new User
            {
                UserId = 4,
                AssignedRoles = new List<Role>(),
            };
            userRepo.GetAllUsers().Add(user);

            UserRepository.RepositoryException exception = Assert.Throws<UserRepository.RepositoryException>(() => userRepo.GetHighestRoleTypeForUser(4));
            Assert.Equal("User has no roles assigned.", exception.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetHighestRoleTypeForUser"/> throws a <see cref="UserRepository.RepositoryException"/> when the user does not exist.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldThrowException_WhenUserDoesNotExist()
        {
            UserRepository.RepositoryException exception = Assert.Throws<UserRepository.RepositoryException>(() => this.userRepo.GetHighestRoleTypeForUser(999));
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Equal("No user found with ID 999", exception.InnerException.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetHighestRoleTypeForUser"/> throws a <see cref="UserRepository.RepositoryException"/> when the assigned role is null.
        /// </summary>
        [Fact]
        public void GetHighestRoleTypeForUser_ShouldThrowException_WhenAssignedRolesIsNull()
        {
            UserRepository userRepo = new UserRepository();
            User user = new User
            {
                UserId = 4,
                AssignedRoles = null,
            };
            userRepo.GetAllUsers().Add(user);

            UserRepository.RepositoryException exception = Assert.Throws<UserRepository.RepositoryException>(() => userRepo.GetHighestRoleTypeForUser(4));
            Assert.Equal("User has no roles assigned.", exception.Message);
            Assert.IsType<ArgumentException>(exception.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUserByID"/> returns the correct user for a given ID.
        /// </summary>
        [Fact]
        public void GetUserByID_ShouldReturnCorrectUser()
        {
            User user = this.userRepo.GetUserByID(1);

            Assert.NotNull(user);
            Assert.Equal(1, user.UserId);
            Assert.Equal("bianca.georgiana.cirnu@gmail.com", user.EmailAddress);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetUserByID"/> throws a <see cref="UserRepository.RepositoryException"/> when the user does not exist.
        /// </summary>
        [Fact]
        public void GetUserByID_ShouldThrowException_WhenUserDoesNotExist()
        {
            UserRepository.RepositoryException exception = Assert.Throws<UserRepository.RepositoryException>(() => this.userRepo.GetUserByID(999));
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Equal("No user found with ID 999", exception.InnerException.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetBannedUsersWhoHaveSubmittedAppeals"/> returns the correct banned users who have submitted appeals.
        /// </summary>
        [Fact]
        public void GetBannedUsersWhoHaveSubmittedAppeals_ShouldReturnCorrectUsers()
        {
            List<User> bannedUsers = this.userRepo.GetBannedUsersWhoHaveSubmittedAppeals();

            Assert.NotNull(bannedUsers);
            Assert.Empty(bannedUsers);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetBannedUsersWhoHaveSubmittedAppeals"/> throws a <see cref="UserRepository.RepositoryException"/> when an exception occurs.
        /// </summary>
        [Fact]
        public void GetBannedUsersWhoHaveSubmittedAppeals_ShouldThrowRepositoryException_WhenExceptionOccurs()
        {
            UserRepository userRepo = new UserRepository();
            System.Reflection.FieldInfo usersListField = typeof(UserRepository).GetField("_usersList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            usersListField.SetValue(userRepo, null); // Simulate a null list

            UserRepository.RepositoryException repoException = Assert.Throws<UserRepository.RepositoryException>(() => userRepo.GetBannedUsersWhoHaveSubmittedAppeals());
            Assert.Equal("Failed to retrieve banned users who have submitted appeals.", repoException.Message);
            Assert.IsType<NullReferenceException>(repoException.InnerException);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.AddRoleToUser"/> adds a role to a user successfully.
        /// </summary>
        [Fact]
        public void AddRoleToUser_ShouldAddRoleSuccessfully()
        {
            Role newRole = new Role(RoleType.Manager, "Manager");

            this.userRepo.AddRoleToUser(1, newRole);
            User user = this.userRepo.GetUserByID(1);

            Assert.Contains(user.AssignedRoles, role => role.RoleType == RoleType.Manager);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.AddRoleToUser"/> throws a <see cref="UserRepository.RepositoryException"/> when the user does not exist.
        /// </summary>
        [Fact]
        public void AddRoleToUser_ShouldThrowException_WhenUserDoesNotExist()
        {
            Role newRole = new Role(RoleType.Manager, "Manager");

            UserRepository.RepositoryException exception = Assert.Throws<UserRepository.RepositoryException>(() => this.userRepo.AddRoleToUser(999, newRole));
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Equal("No user found with ID 999", exception.InnerException.Message);
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetAllUsers"/> returns all users successfully.
        /// </summary>
        [Fact]
        public void GetAllUsers_ShouldReturnAllUsers()
        {
            List<User> users = this.userRepo.GetAllUsers();

            Assert.NotNull(users);
            Assert.Equal(3, users.Count); // Default data contains 3 users
        }

        /// <summary>
        /// Verifies that <see cref="UserRepository.GetAllUsers"/> throws a <see cref="UserRepository.RepositoryException"/> when an exception occurs.
        /// </summary>
        [Fact]
        public void GetAllUsers_ShouldThrowRepositoryException_WhenExceptionOccurs()
        {
            UserRepository userRepo = new UserRepository();
            System.Reflection.FieldInfo usersListField = typeof(UserRepository).GetField("_usersList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            usersListField.SetValue(userRepo, null);

            UserRepository.RepositoryException repoException = Assert.Throws<UserRepository.RepositoryException>(() => userRepo.GetAllUsers());
            Assert.Equal("Failed to retrieve all users.", repoException.Message);
            Assert.IsType<NullReferenceException>(repoException.InnerException);
        }
    }
}