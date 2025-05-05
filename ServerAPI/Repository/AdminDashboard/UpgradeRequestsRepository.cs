// <copyright file="UpgradeRequestsRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using ServerAPI.Data;
    using static Repository.AdminDashboard.UserRepository;

    public class UpgradeRequestsRepository : IUpgradeRequestsRepository
    {
        private readonly DatabaseContext _context;

        public UpgradeRequestsRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            try
            {
                return await _context.UpgradeRequests.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Failed to retrieve all upgrade requests.", ex);
            }
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            try
            {
                var upgradeRequest = await _context.UpgradeRequests
                    .FirstOrDefaultAsync(ur => ur.UpgradeRequestId == upgradeRequestIdentifier);

                if (upgradeRequest == null)
                {
                    throw new ArgumentException($"No upgrade request found with ID {upgradeRequestIdentifier}");
                }

                _context.UpgradeRequests.Remove(upgradeRequest);
                _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to remove upgrade request with ID {upgradeRequestIdentifier}.", ex);
            }
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            try
            {
                var upgradeRequest = await _context.UpgradeRequests
                    .FirstOrDefaultAsync(ur => ur.UpgradeRequestId == upgradeRequestIdentifier);

                if (upgradeRequest == null)
                {
                    throw new ArgumentException($"No upgrade request found with ID {upgradeRequestIdentifier}");
                }

                return upgradeRequest;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Failed to retrieve upgrade request with ID {upgradeRequestIdentifier}.", ex);
            }
        }
    }
}