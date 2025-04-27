// <copyright file="UserTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.Users
{
    using System.Collections.Generic;
    using App1.Models;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="User"/> class.
    /// </summary>
    public class UserTests
    {
        /// <summary>
        /// Verifies that the <see cref="User"/> constructor iSnitializes all properties correctly.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            int userId = 1;
            string emailAddress = "test@example.com";
            string fullName = "Test User";
            int numberOfDeletedReviews = 2;
            bool hasSubmittedAppeal = true;
            List<Role> assignedRoles = new List<Role>
            {
                new Role(RoleType.User, "User"),
            };
            var exception = Record.Exception(() => new User(userId, emailAddress, fullName, numberOfDeletedReviews, hasSubmittedAppeal, assignedRoles));
            Assert.Null(exception);

            User user = new User(userId, emailAddress, fullName, numberOfDeletedReviews, hasSubmittedAppeal, assignedRoles);

            Assert.Equal(userId, user.UserId);
            Assert.Equal(emailAddress, user.EmailAddress);
            Assert.Equal(fullName, user.FullName);
            Assert.Equal(numberOfDeletedReviews, user.NumberOfDeletedReviews);
            Assert.True(user.HasSubmittedAppeal);
            Assert.Equal(assignedRoles, user.AssignedRoles);
        }

        /// <summary>
        /// Verifies that the <see cref="User.ToString"/> method returns the correct string representation of the user.
        /// </summary>
        [Fact]
        public void ToString_ShouldReturnCorrectStringRepresentation()
        {
            User user = new User
            {
                UserId = 1,
                EmailAddress = "test@example.com",
            };

            string result = user.ToString();

            Assert.Equal("Id: 1, email: test@example.com", result);
        }
    }
}
