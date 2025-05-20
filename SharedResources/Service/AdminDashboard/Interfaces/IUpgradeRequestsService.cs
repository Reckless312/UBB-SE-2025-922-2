namespace DataAccess.Service.AdminDashboard.Interfaces
{
    using System.Collections.Generic;
    using DataAccess.Model.AdminDashboard;
    using System.Threading.Tasks;

    public interface IUpgradeRequestsService
    {
        Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests();
        Task ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier);
        Task<string> GetRoleNameBasedOnIdentifier(RoleType roleType);
        Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);
        Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier);
    }
}