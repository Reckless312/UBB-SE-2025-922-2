namespace DataAccess.Service.AdminDashboard.Interfaces
{
    using System.Collections.Generic;
    using DataAccess.Model.AdminDashboard;
    using System.Threading.Tasks;

    public interface IUpgradeRequestsService
    {
        List<UpgradeRequest> RetrieveAllUpgradeRequests();

        void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier);

        string GetRoleNameBasedOnIdentifier(RoleType roleType);

        Task RemoveUpgradeRequestByIdentifierAsync(int upgradeRequestIdentifier);
        Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifierAsync(int upgradeRequestIdentifier);
    }
}