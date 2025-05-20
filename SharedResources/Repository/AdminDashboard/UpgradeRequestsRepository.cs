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
            return await _context.UpgradeRequests.ToListAsync();
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            var upgradeRequest = await _context.UpgradeRequests
                .FirstOrDefaultAsync(ur => ur.UpgradeRequestId == upgradeRequestIdentifier);
                
            if (upgradeRequest != null)
            {
                _context.UpgradeRequests.Remove(upgradeRequest);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            return await _context.UpgradeRequests
                .FirstOrDefaultAsync(ur => ur.UpgradeRequestId == upgradeRequestIdentifier);
        }
    }
}