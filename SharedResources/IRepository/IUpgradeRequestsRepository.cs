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

    /// <summary>
    /// inteface for update requests.
    /// </summary>
    public interface IUpgradeRequestsRepository
    {
        /// <summary>
        /// gets update requests.
        /// </summary>
        /// <returns>list of requests.</returns>
        Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests();

        /// <summary>
        /// removes a request from repo.
        /// </summary>
        /// <param name="upgradeRequestIdentifier">the request to be removed.</param>
        Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);

        Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);
    }
}