// <copyright file="IUpgradeRequestsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    public interface IUpgradeRequestsRepository
    {
        Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests();

        Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);

        Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);
    }
}