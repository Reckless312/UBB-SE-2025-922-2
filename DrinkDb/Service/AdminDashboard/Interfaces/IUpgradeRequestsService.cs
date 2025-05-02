// <copyright file="IUpgradeRequestsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    public interface IUpgradeRequestsService
    {
        Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests();

        Task ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier);

        Task<string> GetRoleNameBasedOnIdentifier(RoleType roleType);
    }
}
