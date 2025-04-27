// <copyright file="UpgradeRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Models
{
    public class UpgradeRequest
    {
        public UpgradeRequest(int upgradeRequestId, int requestingUserIdentifier, string requestingUserDisplayName)
        {
            this.UpgradeRequestId = upgradeRequestId;
            this.RequestingUserIdentifier = requestingUserIdentifier;
            this.RequestingUserDisplayName = requestingUserDisplayName;
        }

        public int UpgradeRequestId { get; set; }

        public int RequestingUserIdentifier { get; set; }

        public string RequestingUserDisplayName { get; set; }

        public override string ToString()
        {
            return this.RequestingUserDisplayName;
        }
    }
}
