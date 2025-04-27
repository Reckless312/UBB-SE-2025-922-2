// <copyright file="IUpgradeRequestsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Services
{
    using System.Collections.Generic;
    using App1.Models;

    public interface IUpgradeRequestsService
    {
        List<UpgradeRequest> RetrieveAllUpgradeRequests();

        void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier);

        string GetRoleNameBasedOnIdentifier(RoleType roleType);
    }
}
