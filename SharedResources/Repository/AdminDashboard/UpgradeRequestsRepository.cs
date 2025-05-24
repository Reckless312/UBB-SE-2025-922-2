namespace Repository.AdminDashboard
{
    using System;
    using System.Collections.Generic;
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using Microsoft.EntityFrameworkCore;
    using ServerAPI.Data;

    public class UpgradeRequestsRepository : IUpgradeRequestsRepository
    {
        private readonly DatabaseContext dataContext;

        public UpgradeRequestsRepository(DatabaseContext context)
        {
            this.dataContext = context;
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            return await dataContext.UpgradeRequests.ToListAsync();
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            UpgradeRequest? upgradeRequest = await this.dataContext.UpgradeRequests.FirstOrDefaultAsync(ur => ur.UpgradeRequestId == upgradeRequestIdentifier);

            if (upgradeRequest == null)
            {
                return;
            }

            this.dataContext.UpgradeRequests.Remove(upgradeRequest);
            await this.dataContext.SaveChangesAsync();
        }

        public async Task<UpgradeRequest?> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            return await this.dataContext.UpgradeRequests.FirstOrDefaultAsync(ur => ur.UpgradeRequestId == upgradeRequestIdentifier);
        }
    }
}