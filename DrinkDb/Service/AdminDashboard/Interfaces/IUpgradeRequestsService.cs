// <copyright file="IUpgradeRequestsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using System.Collections.Generic;
    using DataAccess.Model.AdminDashboard;

    public interface IUpgradeRequestsService
    {
        List<UpgradeRequest> RetrieveAllUpgradeRequests();

        void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier);

        string GetRoleNameBasedOnIdentifier(RoleType roleType);
    }
}