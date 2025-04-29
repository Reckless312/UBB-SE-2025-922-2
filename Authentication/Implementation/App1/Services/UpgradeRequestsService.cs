namespace App1.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using App1.Models;
    using App1.Repositories;

    public class UpgradeRequestsService : IUpgradeRequestsService
    {
        private readonly IUpgradeRequestsRepository upgradeRequestsRepository;
        private readonly IRolesRepository rolesRepository;
        private readonly IUserRepository userRepository;

        public UpgradeRequestsService(
            IUpgradeRequestsRepository upgradeRequestsRepository,
            IRolesRepository rolesRepository,
            IUserRepository userRepository)
        {
            this.upgradeRequestsRepository = upgradeRequestsRepository;
            this.rolesRepository = rolesRepository;
            this.userRepository = userRepository;
            this.RemoveUpgradeRequestsFromBannedUsers();
        }

        public void RemoveUpgradeRequestsFromBannedUsers()
        {
            List<UpgradeRequest> pendingUpgradeRequests = this.RetrieveAllUpgradeRequests();

            // Use a reversed loop or a copy of the list to safely remove items
            for (int i = pendingUpgradeRequests.Count - 1; i >= 0; i--)
            {
                int requestingUserIdentifier = pendingUpgradeRequests[i].RequestingUserIdentifier;
                if (this.userRepository.GetHighestRoleTypeForUser(requestingUserIdentifier) == RoleType.Banned)
                {
                    this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(pendingUpgradeRequests[i].UpgradeRequestId);
                }
            }
        }

        public string GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            List<Role> availableRoles = this.rolesRepository.GetAllRoles();
            Role matchingRole = availableRoles.First(role => role.RoleType == roleType);
            return matchingRole.RoleName;
        }

        public List<UpgradeRequest> RetrieveAllUpgradeRequests()
        {
            return this.upgradeRequestsRepository.RetrieveAllUpgradeRequests();
        }

        public void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            if (isRequestAccepted)
            {
                UpgradeRequest currentUpgradeRequest = this.upgradeRequestsRepository.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
                int requestingUserIdentifier = currentUpgradeRequest.RequestingUserIdentifier;
                RoleType currentHighestRoleType = this.userRepository.GetHighestRoleTypeForUser(requestingUserIdentifier);
                Role nextRoleLevel = this.rolesRepository.GetNextRoleInHierarchy(currentHighestRoleType);
                this.userRepository.AddRoleToUser(requestingUserIdentifier, nextRoleLevel);
            }

            this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }
    }
}