// <copyright file="UserTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.Users
{
    using DataAccess.Model.AdminDashboard;
    using DataAccess.Model.Authentication;
    using System;
    using System.Collections.Generic;
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
            Guid userId = new Guid();
            string emailAddress = "test@example.com";
            string fullName = "Test User";
            int numberOfDeletedReviews = 2;
            bool hasSubmittedAppeal = true;
            List<Role> assignedRoles = new List<Role>
            {
                new Role(RoleType.User, "User"),
            };
            var exception = Record.Exception(() => new User { UserId = userId, EmailAddress = emailAddress, Username = fullName, NumberOfDeletedReviews = numberOfDeletedReviews, HasSubmittedAppeal = hasSubmittedAppeal, PasswordHash = String.Empty, TwoFASecret = String.Empty });
            Assert.Null(exception);

            User user = new User { UserId = userId, EmailAddress = emailAddress, Username = fullName, NumberOfDeletedReviews = numberOfDeletedReviews, HasSubmittedAppeal = hasSubmittedAppeal, PasswordHash = String.Empty, TwoFASecret = String.Empty };

            Assert.Equal(userId, user.UserId);
            Assert.Equal(emailAddress, user.EmailAddress);
            Assert.Equal(fullName, user.Username);
            Assert.Equal(numberOfDeletedReviews, user.NumberOfDeletedReviews);
            Assert.True(user.HasSubmittedAppeal);
        }
    }
}
