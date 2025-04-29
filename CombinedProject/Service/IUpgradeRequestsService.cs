namespace CombinedProject.Service
{
    using System.Collections.Generic;
    using CombinedProject.Model;

    public interface IUpgradeRequestsService
    {
        List<UpgradeRequest> RetrieveAllUpgradeRequests();

        void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier);

        string GetRoleNameBasedOnIdentifier(RoleType roleType);
    }
}
