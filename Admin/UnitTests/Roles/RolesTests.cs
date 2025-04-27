// <copyright file="RolesTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.Roles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using App1.Models;
    using App1.Repositories;
    using Xunit;

    public class RolesTests
    {
        [Fact]
        public void Role_WhenCreated_InitializesCorrectly()
        {
            var testCases = new[]
            {
                new { Type = RoleType.Banned, Name = "Banned" },
                new { Type = RoleType.User, Name = "User" },
                new { Type = RoleType.Admin, Name = "Admin" },
                new { Type = RoleType.Manager, Name = "Manager" },
            };

            foreach (var testCase in testCases)
            {
                var role = new Role(testCase.Type, testCase.Name);

                Assert.Equal(testCase.Type, role.RoleType);
                Assert.Equal(testCase.Name, role.RoleName);
            }
        }

        [Fact]
        public void Role_WhenPropertiesModified_UpdatesCorrectly()
        {
            var role = new Role(RoleType.User, "User");

            role.RoleType = RoleType.Admin;
            role.RoleName = "Admin";

            Assert.Equal(RoleType.Admin, role.RoleType);
            Assert.Equal("Admin", role.RoleName);
        }
    }
}
