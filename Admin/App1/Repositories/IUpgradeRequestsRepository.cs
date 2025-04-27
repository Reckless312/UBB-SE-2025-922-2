// <copyright file="IUpgradeRequestsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using App1.Models;

    /// <summary>
    /// inteface for update requests.
    /// </summary>
    public interface IUpgradeRequestsRepository
    {
        /// <summary>
        /// gets update requests.
        /// </summary>
        /// <returns>list of requests.</returns>
        List<UpgradeRequest> RetrieveAllUpgradeRequests();

        /// <summary>
        /// removes a request from repo.
        /// </summary>
        /// <param name="upgradeRequestIdentifier">the request to be removed.</param>
        void RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);

        UpgradeRequest RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);
    }
}
