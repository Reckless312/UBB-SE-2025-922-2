namespace IRepository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataAccess.Model.AdminDashboard;

    public interface IUpgradeRequestsRepository
    {
        Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests();

        Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);

        Task<UpgradeRequest?> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);
    }
}