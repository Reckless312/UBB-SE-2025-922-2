// <copyright file="UpgradeRequestTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.UpgradeRequests
{
    using App1.Models;

    public class UpgradeRequestTests
    {
        [Fact]
        public void Constructor_WithValidParameters_CreatesUpgradeRequestWithCorrectProperties()
        {
            int expectedUpgradeRequestId = 1;
            int expectedRequestingUserIdentifier = 100;
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
            int requestingUserIdentifier = 100;
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