// <copyright file="UpgradeRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace DrinkDb_Auth.Model.AdminDashboard
{
    public class UpgradeRequest
    {
        public UpgradeRequest(int upgradeRequestId, Guid requestingUserIdentifier, string requestingUserDisplayName)
        {
            UpgradeRequestId = upgradeRequestId;
            RequestingUserIdentifier = requestingUserIdentifier;
            RequestingUserDisplayName = requestingUserDisplayName;
        }

        public int UpgradeRequestId { get; set; }

        public Guid RequestingUserIdentifier { get; set; }

        public string RequestingUserDisplayName { get; set; }

        public override string ToString()
        {
            return RequestingUserDisplayName;
        }
    }
}
