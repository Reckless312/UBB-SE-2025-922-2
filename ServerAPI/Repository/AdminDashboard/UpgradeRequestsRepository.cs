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
            return _context.UpgradeRequests.ToListAsync().Result;
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
                List<UpgradeRequest> upgradeRequests =  _context.UpgradeRequests.ToListAsync().Result;
                UpgradeRequest upgradeRequest = upgradeRequests.Where(upgrade => upgrade.UpgradeRequestId == upgradeRequestIdentifier).First();
                _context.UpgradeRequests.Remove(upgradeRequest);
                _context.SaveChanges();
          
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            var upgradeRequest = _context.UpgradeRequests.FirstOrDefaultAsync(ur => ur.UpgradeRequestId == upgradeRequestIdentifier).Result;
            return upgradeRequest;
        }
    }
}