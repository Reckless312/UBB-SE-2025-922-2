// <copyright file="UpgradeRequestTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using DataAccess.Model.AdminDashboard;
using System;
using Xunit;

namespace UnitTests.UpgradeRequests
{
    public class UpgradeRequestTests
    {
        [Fact]
        public void Constructor_WithValidParameters_CreatesUpgradeRequestWithCorrectProperties()
        {
            int expectedUpgradeRequestId = 1;
            Guid expectedRequestingUserIdentifier = Guid.NewGuid();
            string expectedRequestingUserDisplayName = "Test User";
            UpgradeRequest upgradeRequest = new UpgradeRequest(
                expectedUpgradeRequestId,
                expectedRequestingUserIdentifier,
                expectedRequestingUserDisplayName);
            Assert.Equal(expectedUpgradeRequestId, upgradeRequest.UpgradeRequestId);
            Assert.Equal(expectedRequestingUserIdentifier, upgradeRequest.RequestingUserIdentifier);
            Assert.Equal(expectedRequestingUserDisplayName, upgradeRequest.RequestingUserDisplayName);
        }

        [Fact]
        public void ToString_ReturnsRequestingUserDisplayName()
        {
            int upgradeRequestId = 1;
            Guid requestingUserIdentifier = Guid.NewGuid();
            string expectedDisplayName = "Test User";
            UpgradeRequest upgradeRequest = new UpgradeRequest(
                upgradeRequestId,
                requestingUserIdentifier,
                expectedDisplayName);

            string result = upgradeRequest.ToString();

            Assert.Equal(expectedDisplayName, result);
        }
    }
}